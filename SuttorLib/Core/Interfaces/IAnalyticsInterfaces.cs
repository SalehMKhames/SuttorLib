using System.Threading.Tasks;
using SuttorLibrary.Models.Analytics;
using System.Collections.Generic;

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