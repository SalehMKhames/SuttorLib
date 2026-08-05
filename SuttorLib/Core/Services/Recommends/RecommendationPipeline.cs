using SuttorLib.Models.Recommender;
using SuttorLib.Core.Services.Recommends;
using SuttorLibrary.Core;
using SuttorLib.Core.Services.Recommends.Mongo;

namespace LibrarySystem.Recommendations.Services
{
    /// <summary>
    /// The end-to-end job: extract from MySQL -> train -> score every user
    /// with either matrix factorization or the interest-based fallback ->
    /// upsert results into MongoDB. Run this via the /train endpoint for now;
    /// later you can move the call into a scheduled BackgroundService.
    /// </summary>
    public class RecommendationPipelineService
    {
        private const int TopN = 10;
        private readonly IUnitOfWork _unit;
        private readonly MatrixFactorizationRecommenderService _mfService;
        private readonly InterestsBasedRecommenderService _fallbackService;
        private readonly IRecommendRepo _repository;

        public RecommendationPipelineService(
            ILogger<RecommendationPipelineService> logger,
            IUnitOfWork dataAccess,
            MatrixFactorizationRecommenderService mfService,
            InterestsBasedRecommenderService fallbackService,
            IRecommendRepo repository)
        {
            _unit = dataAccess;
            _mfService = mfService;
            _fallbackService = fallbackService;
            _repository = repository;
        }

        public async Task RunAsync()
        {
            var interactions = await _unit.UserRepo.GetAllDownloadInteractionsAsync();

            // GET the interests of the user (user id, category id)
            var interests = await _unit.UserRepo.GetAllUserInterestsAsync();

            //GET all users' Ids
            var Ids = await _unit.UserRepo.GetAll();
            var userIds = Ids.Select(u => u.Id).ToList();

            // Train the model on interactions data.
            var model = _mfService.Train(interactions);

            // Only books with at least one download have a learned MF vector.
            var knownBookIds = interactions.Select(i => i.BookId).ToHashSet();

            var downloadedByUser = interactions
                        .GroupBy(i => i.UserId)
                        .ToDictionary(g => g.Key, g => g.Select(i => i.BookId).ToHashSet());

            foreach (var userId in userIds)
            {
                var downloadCount = downloadedByUser.TryGetValue(userId, out var downloaded) ? downloaded.Count : 0;

                List<RecommendedBook> recommended;
                string source;

                if (model != null && downloadCount >= MatrixFactorizationRecommenderService.MinDownloadsForMf)
                {
                    var alreadyDownloaded = downloadedByUser[userId];
                    var candidates = knownBookIds
                                .Where(bookId => !alreadyDownloaded.Contains(bookId))
                                .Select(bookId => new DownloadAnalysis { UserId = userId, BookId = bookId, Label = 0f })
                                .ToList();

                    var scored = _mfService.ScoreCandidates(model, candidates);

                    recommended = scored
                                .OrderByDescending(s => s.Score)
                                .Take(TopN)
                                .Select(s => new RecommendedBook { BookId = s.BookId, Score = s.Score })
                                .ToList();
                    source = "MatrixFactorization";
                }
                else
                {
                    var fallback = _fallbackService.Recommend(userId, interests, interactions, TopN);
                    recommended = fallback
                                .Select(f => new RecommendedBook { BookId = f.BookId, Score = f.Score })
                                .ToList();
                    source = "InterestFallback";
                }

                if (recommended.Count == 0)
                    continue; // nothing to offer this user yet (no data at all in the system)

                await _repository.UpsertAsync(new BookRecommendationDocument
                {
                    UserId = userId,
                    RecommendedBooks = recommended,
                    Source = source,
                    GeneratedAtUtc = DateTime.UtcNow
                });
            }
            
        }
    }
}
