using SuttorLibrary.Models.Analytics;

namespace SuttorLib.Core.Services.Statistics
{
    public interface IStatisticsCalculator
    {
        Task CalculateDailyStatisticsAsync();
        Task<IEnumerable<PlatformStatistic>> GetLatestStatisticsAsync();
    }

    public interface IRecommendationEngine
    {
        Task GenerateUserRecommendationsAsync();
        Task<IEnumerable<UserRecommendation>> GetRecommendationsForUserAsync(string userId, string itemType);
    }
}