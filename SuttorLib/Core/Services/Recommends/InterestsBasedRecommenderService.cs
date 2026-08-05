using SuttorLibrary.Core.Interfaces;

namespace SuttorLib.Core.Services.Recommends
{
    // Score here is a download count (used for ranking), not an ML prediction.
    public record FallbackRecommendation(string BookId, int Score);

    public class InterestsBasedRecommenderService
    {
        public List<FallbackRecommendation> Recommend(
            string userId,
            List<UserInterestRow> allInterests,
            List<InteractionRow> allInteractions,
            int topN = 10)
        {
            // Get what the passed user has been downloaded as HashSet.
            var alreadyDownloaded = allInteractions
                .Where(i => i.UserId == userId)
                .Select(i => i.BookId)
                .ToHashSet();

            // Get the passed user's interests as HashSet.
            var myCategories = allInterests
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.CategoryId)
                .ToHashSet();

            IEnumerable<string> candidateBookIds;

            if (myCategories.Count > 0)
            {
                // Get the other users who have the same interests.
                var similarUserIds = allInterests
                    .Where(ui => ui.UserId != userId && myCategories.Contains(ui.CategoryId))
                    .Select(ui => ui.UserId)
                    .ToHashSet();

                // Get the books that a user has downloaded
                candidateBookIds = allInteractions
                    .Where(i => similarUserIds.Contains(i.UserId))
                    .Select(i => i.BookId);
            }
            else
            {
                // No declared interests at all yet: go straight to system-wide popularity.
                candidateBookIds = allInteractions.Select(i => i.BookId);
            }

            // Get the candidate books except what the passed user has downloaded.
            // Group them by the book id, and rank them decsending by score.
            var ranked = candidateBookIds
                .Where(bookId => !alreadyDownloaded.Contains(bookId))
                .GroupBy(bookId => bookId)
                .Select(g => new FallbackRecommendation(g.Key, g.Count()))
                .OrderByDescending(r => r.Score)
                .Take(topN)
                .ToList();

            // A newly-interested user's category group might be too small to
            // fill topN on its own — top up with global popularity.
            if (ranked.Count < topN && myCategories.Count > 0)
            {
                var alreadyRanked = ranked.Select(r => r.BookId).ToHashSet();

                var globalTopUp = allInteractions
                    .Select(i => i.BookId)
                    .Where(bookId => !alreadyDownloaded.Contains(bookId) && !alreadyRanked.Contains(bookId))
                    .GroupBy(bookId => bookId)
                    .Select(g => new FallbackRecommendation(g.Key, g.Count()))
                    .OrderByDescending(r => r.Score)
                    .Take(topN - ranked.Count);

                ranked.AddRange(globalTopUp);
            }

            return ranked;
        }
    }
}
