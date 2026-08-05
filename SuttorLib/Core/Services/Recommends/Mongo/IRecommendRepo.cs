using SuttorLib.Models.Recommender;

namespace SuttorLib.Core.Services.Recommends.Mongo
{
    public interface IRecommendRepo
    {
        public Task UpsertAsync(BookRecommendationDocument document);
        public Task<BookRecommendationDocument?> GetForUserAsync(string userId);
    }
}
