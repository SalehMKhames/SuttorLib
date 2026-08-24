namespace SuttorLibrary.DTOs
{
    // ---------------- User Dashboard ----------------

    public record MyReadingDto(
        int DownloadedBooks,
        int FinishedBooks,
        int FavoriteBooks,
        int RatingsGiven,
        double AverageRatingGiven,
        int CommentsWritten
    );

    public record CategoryShareDto(string CategoryId, string CategoryName, double Percent);

    public record UserDashboardDto(
        MyReadingDto Reading,
        List<CategoryShareDto> Interests,
        string? Segment,
        string? SegmentDescription
    );

    public record BecauseYouReadDto(string SourceBookTitle, List<string> RecommendedTitles);

    // ---------------- Segmentation / Clustering ----------------

    public record ClusterSummaryDto(
        string Label,
        int UserCount,
        double Percentage,
        double AvgDownloads,
        double AvgFinishedReads,
        double AvgFavorites,
        double AvgRatings,
        double AvgComments
    );

    public record SegmentationResultDto(
        int ClusterCount,
        List<ClusterSummaryDto> Clusters,
        Dictionary<string, string> UserSegments
    );

    // ---------------- Engagement ----------------

    public record ActiveUserDto(
        string UserId,
        string UserName,
        int Reads,
        int Favorites,
        int Ratings,
        int Comments,
        double EngagementScore
    );

    // ---------------- Books ----------------

    public record TopBookDto(
        string BookId,
        string Title,
        long Count,
        double? AvgRating,
        int? RatingCount
    );

    public record AssociationRuleDto(
        string AntecedentTitle,
        string ConsequentTitle,
        double Support,
        double Confidence,
        double Lift
    );

    public record CategoryAssociationDto(
        string AntecedentCategory,
        string ConsequentCategory,
        double Support,
        double Confidence,
        double Lift
    );

    public record TopBooksDto(
        List<TopBookDto> MostDownloaded,
        List<TopBookDto> MostRead,
        List<TopBookDto> MostFavorited,
        List<TopBookDto> HighestRated
    );

    // ---------------- Authors ----------------

    public record AuthorStatDto(
        string AuthorId,
        string Name,
        long Value,
        double EngagementScore
    );

    public record TopAuthorsDto(
        List<AuthorStatDto> ByPublishedBooks,
        List<AuthorStatDto> ByReads,
        List<AuthorStatDto> ByEngagement
    );

    // ---------------- Author Dashboard ----------------

    public record AuthorBookPerformanceDto(
        string BookId,
        string Title,
        int Downloads,
        int FinishedReads,
        int Favorites,
        int RatingCount,
        double AvgRating
    );

    public record RatingDistributionDto(
        int FiveStars,
        int FourStars,
        int ThreeStars,
        int TwoStars,
        int OneStar,
        int Total
    );

    public record AudienceInsightsDto(
        int UniqueReaders,
        List<CategoryShareDto> InterestedCategories,
        List<string> PopularContentTitles
    );

    public record AuthorDashboardDto(
        int PublishedBooks,
        int TotalDownloads,
        int TotalFinishedReads,
        int TotalFavorites,
        double AvgRating,
        int RatingCount,
        RatingDistributionDto RatingDistribution,
        List<AuthorBookPerformanceDto> BookPerformance,
        AudienceInsightsDto Audience
    );

    // ---------------- Admin Overview ----------------

    public record MonthlyGrowthDto(
        long CurrentMonth,
        long PreviousMonth,
        double GrowthPercent
    );

    public record PlatformOverviewDto(
        long TotalUsers,
        long NewUsersThisMonth,
        double UserGrowthPercent,
        long TotalAuthors,
        long NewAuthorsThisMonth,
        long TotalBooks,
        long TotalDownloads,
        long TotalRatings,
        long TotalInteractions,
        MonthlyGrowthDto DownloadsGrowth,
        MonthlyGrowthDto InteractionsGrowth
    );

    public record TimeSeriesPointDto(string Period, long Count);

    public record PlatformGrowthDto(
        List<TimeSeriesPointDto> UserGrowth,
        List<TimeSeriesPointDto> BookGrowth,
        List<TimeSeriesPointDto> DownloadActivity,
        List<TimeSeriesPointDto> RatingActivity
    );

    // ---------------- Anomaly Detection ----------------

    public record AnomalousUserDto(
        string UserId,
        string UserName,
        int Downloads,
        int FinishedReads,
        int Favorites,
        int Ratings,
        int Comments,
        double AnomalyScore
    );
}
