using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Interfaces;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IStatisticsCalculator _statisticsCalculator;
        private readonly IRecommendationEngine _recommendationEngine;

        public AnalyticsController(
            IStatisticsCalculator statisticsCalculator, 
            IRecommendationEngine recommendationEngine)
        {
            _statisticsCalculator = statisticsCalculator;
            _recommendationEngine = recommendationEngine;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetLatestStatistics()
        {
            var stats = await _statisticsCalculator.GetLatestStatisticsAsync();
            return Ok(stats);
        }

        [Authorize]
        [HttpGet("recommendations/{userId}/{itemType}")]
        public async Task<IActionResult> GetRecommendations(string userId, string itemType)
        {
            var recs = await _recommendationEngine.GetRecommendationsForUserAsync(userId, itemType);
            return Ok(recs);
        }
    }
}