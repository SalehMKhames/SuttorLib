using SuttorLibrary.Models.Analytics;

namespace SuttorLib.Core.Interfaces
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