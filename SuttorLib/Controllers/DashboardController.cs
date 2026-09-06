using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Services.Analytics;
using System.Security.Claims;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(IAnalyticsService analytics) : ControllerBase
    {
        private readonly IAnalyticsService _analytics = analytics;

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim missing.");

        // ---------------- User ----------------

        /// <summary>My Reading stats, My Interests and my behavioral segment (K-Means).</summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserDashboard()
            => Ok(await _analytics.GetUserDashboardAsync(CurrentUserId));

        /// <summary>"Because you read X" recommendations powered by FP-Growth association rules.</summary>
        [HttpGet("user/because-you-read")]
        [Authorize]
        public async Task<IActionResult> GetBecauseYouRead([FromQuery] int maxSources = 3)
            => Ok(await _analytics.GetBecauseYouReadAsync(CurrentUserId, maxSources));

        // ---------------- Author ----------------

        /// <summary>Content performance, rating analysis and audience insights for an author.</summary>
        [HttpGet("author/{authorId}")]
        [Authorize(Roles = "Author,Admin")]
        public async Task<IActionResult> GetAuthorDashboard(string authorId)
        {
            var result = await _analytics.GetAuthorDashboardAsync(authorId);
            return result == null ? NotFound("Author not found.") : Ok(result);
        }

        // ---------------- Admin ----------------

        /// <summary>Platform KPIs with current vs previous month growth.</summary>
        [HttpGet("admin/overview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverview()
            => Ok(await _analytics.GetPlatformOverviewAsync());

        /// <summary>Time-series platform growth (users, books, downloads, ratings).</summary>
        [HttpGet("admin/growth")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGrowth([FromQuery] int months = 12)
            => Ok(await _analytics.GetPlatformGrowthAsync(months));

        /// <summary>K-Means user segmentation with per-cluster statistics.</summary>
        [HttpGet("admin/users/segments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserSegments([FromQuery] int k = 3)
            => Ok(await _analytics.GetUserSegmentsAsync(Math.Clamp(k, 2, 6)));

        /// <summary>Most active users ranked by configurable engagement score.</summary>
        [HttpGet("admin/users/top")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopUsers([FromQuery] int top = 5)
            => Ok(await _analytics.GetMostActiveUsersAsync(top));

        /// <summary>Top downloaded / read / favorite / rated books.</summary>
        [HttpGet("admin/books/top")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopBooks([FromQuery] int top = 5)
            => Ok(await _analytics.GetTopBooksAsync(top));

        /// <summary>"Frequently read together" rules (FP-Growth over-book transactions).</summary>
        [HttpGet("admin/books/rules")]
        [Authorize(Roles = "Author, Admin")]
        public async Task<IActionResult> GetBookRules([FromQuery] double minSupport = 0.02, [FromQuery] double minConfidence = 0.3)
            => Ok(await _analytics.GetBookAssociationRulesAsync(minSupport, minConfidence));

        /// <summary>Category-to-category associations (FP-Growth over category sets).</summary>
        [HttpGet("admin/categories/rules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryRules([FromQuery] double minSupport = 0.02, [FromQuery] double minConfidence = 0.3)
            => Ok(await _analytics.GetCategoryAssociationRulesAsync(minSupport, minConfidence));

        /// <summary>Top authors by published books, reads and engagement.</summary>
        [HttpGet("admin/authors/top")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopAuthors([FromQuery] int top = 5)
            => Ok(await _analytics.GetTopAuthorsAsync(top));

        /// <summary>Anomalously-behaving users flagged by robust z-score detection.</summary>
        [HttpGet("admin/anomalies")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAnomalies([FromQuery] double threshold = 3.0)
            => Ok(await _analytics.DetectAnomaliesAsync(threshold));
    }
}
