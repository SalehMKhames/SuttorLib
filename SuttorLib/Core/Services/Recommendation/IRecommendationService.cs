using SuttorLib.Models.Library;

namespace SuttorLib.Core.Services
{
    public interface IRecommendationService
    {
        void TrainModel(IEnumerable<BookRatingData> trainingData);
        float PredictRating(string userId, int bookId);
        IEnumerable<int> GetTopRecommendations(string userId, IEnumerable<int> allBookIds, int topCount = 5);
    }
}
