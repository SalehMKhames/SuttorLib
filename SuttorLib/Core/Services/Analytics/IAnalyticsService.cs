using SuttorLibrary.DTOs;

namespace SuttorLib.Core.Services.Analytics
{
    public interface IAnalyticsService
    {
        // User dashboard
        Task<UserDashboardDto> GetUserDashboardAsync(string userId);
        Task<List<BecauseYouReadDto>> GetBecauseYouReadAsync(string userId, int maxSources = 3);

        // Author dashboard
        Task<AuthorDashboardDto?> GetAuthorDashboardAsync(string authorId);

        // Admin - overview / growth
        Task<PlatformOverviewDto> GetPlatformOverviewAsync();
        Task<PlatformGrowthDto> GetPlatformGrowthAsync(int months = 12);

        // Admin - data mining
        Task<List<ActiveUserDto>> GetMostActiveUsersAsync(int top = 5);
        Task<TopBooksDto> GetTopBooksAsync(int top = 5);
        Task<List<AssociationRuleDto>> GetBookAssociationRulesAsync(double minSupport = 0.02, double minConfidence = 0.3);
        Task<List<CategoryAssociationDto>> GetCategoryAssociationRulesAsync(double minSupport = 0.02, double minConfidence = 0.3);
        Task<SegmentationResultDto> GetUserSegmentsAsync(int k = 3);
        Task<List<AnomalousUserDto>> DetectAnomaliesAsync(double threshold = 3.0, int top = 10);
        Task<TopAuthorsDto> GetTopAuthorsAsync(int top = 5);

        // Shared
        Task<List<UserFeatureRow>> GetUserFeaturesAsync();
    }
}
