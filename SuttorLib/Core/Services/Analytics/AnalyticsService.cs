using Microsoft.EntityFrameworkCore;
using SuttorLib.Core.Services.Analytics;
using SuttorLib.Models.Library;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;

namespace SuttorLib.Core.Services.Analytics
{
    /// <summary>
    /// Implements the statistics + data-mining layer described in
    /// data_mining_platform_dashboards.md. All mining is computed on demand
    /// from MySQL; results are cheap enough at this scale to compute per request.
    /// </summary>
    public class AnalyticsService(AppDbContext db, IConfiguration configuration) : IAnalyticsService
    {
        private readonly AppDbContext _db = db;
        private readonly IConfiguration _configuration = configuration;

        // Configurable engagement weights: read=+1, like=+1, rating=+2, comment=+3
        private (double Read, double Like, double Rating, double Comment) Weights =>
        (
            _configuration.GetValue("Analytics:Weights:Read", 1.0),
            _configuration.GetValue("Analytics:Weights:Like", 1.0),
            _configuration.GetValue("Analytics:Weights:Rating", 2.0),
            _configuration.GetValue("Analytics:Weights:Comment", 3.0)
        );

        // ------------------------------------------------------------------
        // Shared feature extraction
        // ------------------------------------------------------------------

        public async Task<List<UserFeatureRow>> GetUserFeaturesAsync()
        {
            var downloads = await _db.Downloads.AsNoTracking()
                .Select(d => new { d.UserID, d.IsFinishReading })
                .ToListAsync();
            var favorites = await _db.FavoriteBooks.AsNoTracking()
                .Select(f => f.UserId).ToListAsync();
            var ratings = await _db.BookRatings.AsNoTracking()
                .Select(r => new { r.UserId, r.Comment }).ToListAsync();

            var users = await _db.Users.AsNoTracking()
                .Select(u => new { u.Id, u.UserName }).ToListAsync();

            var map = users.ToDictionary(u => u.Id, u => new UserFeatureRow
            {
                UserId = u.Id,
                UserName = u.UserName
            });

            foreach (var d in downloads)
            {
                if (!map.TryGetValue(d.UserID, out var row)) continue;
                row.Downloads++;
                if (d.IsFinishReading) row.FinishedReads++;
            }
            foreach (var f in favorites)
                if (map.TryGetValue(f, out var row)) row.Favorites++;
            foreach (var r in ratings)
            {
                if (!map.TryGetValue(r.UserId, out var row)) continue;
                row.Ratings++;
                if (!string.IsNullOrWhiteSpace(r.Comment)) row.Comments++;
            }

            return map.Values.ToList();
        }

        // ------------------------------------------------------------------
        // User dashboard
        // ------------------------------------------------------------------

        public async Task<UserDashboardDto> GetUserDashboardAsync(string userId)
        {
            var downloads = await _db.Downloads.CountAsync(d => d.UserID == userId);
            var finished = await _db.Downloads.CountAsync(d => d.UserID == userId && d.IsFinishReading);
            var favorites = await _db.FavoriteBooks.CountAsync(f => f.UserId == userId);
            var userRatings = await _db.BookRatings.AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => new { r.Rating, r.Comment })
                .ToListAsync();

            MyReadingDto reading = new(
                DownloadedBooks: downloads,
                FinishedBooks: finished,
                FavoriteBooks: favorites,
                RatingsGiven: userRatings.Count,
                AverageRatingGiven: userRatings.Count == 0 ? 0 : Math.Round(userRatings.Average(r => (double)r.Rating), 2),
                CommentsWritten: userRatings.Count(r => !string.IsNullOrWhiteSpace(r.Comment))
            );

            var interests = await GetCategorySharesAsync(userId);

            string? segment = null;
            string? description = null;
            try
            {
                var segmentation = await GetUserSegmentsAsync();
                if (segmentation.UserSegments.TryGetValue(userId, out var label))
                {
                    segment = label;
                    description = label switch
                    {
                        "Casual Reader" => "You read occasionally - try building a reading habit!",
                        "Active Reader" => "You are an engaged member of the community.",
                        "Book Enthusiast" => "You are one of the most devoted readers on the platform.",
                        _ => null
                    };
                }
            }
            catch
            {
                // Segmentation needs enough users; dashboard still works without it.
            }

            return new UserDashboardDto(reading, interests, segment, description);
        }

        /// <summary>User's category distribution, weighted across downloads/reads/favorites/ratings.</summary>
        public async Task<List<CategoryShareDto>> GetCategorySharesAsync(string userId)
        {
            const double wDownload = 1.0, wFinish = 2.0, wFavorite = 3.0, wRating = 2.0;

            var bookWeights = new Dictionary<string, double>();

            var dl = await _db.Downloads.AsNoTracking()
                .Where(d => d.UserID == userId)
                .Select(d => new { d.BookID, d.IsFinishReading })
                .ToListAsync();
            foreach (var d in dl)
                bookWeights[d.BookID] = bookWeights.GetValueOrDefault(d.BookID) + wDownload + (d.IsFinishReading ? wFinish : 0);

            var favs = await _db.FavoriteBooks.AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.BookId)
                .ToListAsync();
            foreach (var b in favs)
                bookWeights[b] = bookWeights.GetValueOrDefault(b) + wFavorite;

            var ratedBooks = await _db.BookRatings.AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => r.BookId)
                .ToListAsync();
            foreach (var b in ratedBooks)
                bookWeights[b] = bookWeights.GetValueOrDefault(b) + wRating;

            if (bookWeights.Count == 0) return [];

            var bookIds = bookWeights.Keys.ToList();
            var bookCats = await _db.BookCategories.AsNoTracking()
                .Where(bc => bookIds.Contains(bc.bookId))
                .Join(_db.Categories, bc => bc.categoryId, c => c.Id,
                    (bc, c) => new { bc.bookId, c.Id, c.Name })
                .ToListAsync();

            var catTotals = new Dictionary<string, (double Weight, string Name)>();
            foreach (var bc in bookCats)
            {
                var w = bookWeights[bc.bookId];
                var cur = catTotals.GetValueOrDefault(bc.Id, (0, bc.Name));
                catTotals[bc.Id] = (cur.Weight + w, bc.Name);
            }

            double grandTotal = catTotals.Values.Sum(v => v.Weight);
            if (grandTotal == 0) return [];

            return catTotals
                .OrderByDescending(kv => kv.Value.Weight)
                .Select(kv => new CategoryShareDto(kv.Key, kv.Value.Name,
                    Math.Round(kv.Value.Weight / grandTotal * 100, 1)))
                .ToList();
        }

        /// <summary>"Because you read X" powered by FP-Growth over the whole platform.</summary>
        public async Task<List<BecauseYouReadDto>> GetBecauseYouReadAsync(string userId, int maxSources = 3)
        {
            var (transactions, labels, userTransactions) = await BuildBookTransactionsAsync();
            var rules = FpGrowthService.Mine(transactions, labels);
            if (rules.Count == 0) return [];

            var readBooks = userTransactions.TryGetValue(userId, out var set) ? set : new HashSet<string>();
            if (readBooks.Count == 0) return [];

            var labelToId = labels.ToDictionary(kv => kv.Value, kv => kv.Key);
            var titles = await BookTitlesMapAsync();

            var bySource = rules
                .Where(r => readBooks.Contains(labelToId.GetValueOrDefault(r.AntecedentTitle, "")))
                .GroupBy(r => r.AntecedentTitle)
                .Take(maxSources);

            return bySource.Select(g => new BecauseYouReadDto(
                SourceBookTitle: g.Key,
                RecommendedTitles: g
                    .OrderByDescending(r => r.Confidence * r.Lift)
                    .Select(r => r.ConsequentTitle)
                    .Where(t => !readBooks.Contains(labelToId.GetValueOrDefault(t, "")))
                    .Take(5)
                    .ToList()
            )).ToList();
        }

        // ------------------------------------------------------------------
        // Author dashboard
        // ------------------------------------------------------------------

        public async Task<AuthorDashboardDto?> GetAuthorDashboardAsync(string authorId)
        {
            var authorExists = await _db.Authors.AnyAsync(a => a.Id == authorId);
            if (!authorExists) return null;

            var books = await _db.BookAuthors.AsNoTracking()
                .Where(ba => ba.Author_Id == authorId)
                .Join(_db.Books, ba => ba.Book_Id, b => b.Id,
                    (ba, b) => new { b.Id, b.Title })
                .Distinct()
                .ToListAsync();
            var bookIds = books.Select(b => b.Id).ToList();
            if (bookIds.Count == 0)
                return new AuthorDashboardDto(0, 0, 0, 0, 0, 0,
                    new RatingDistributionDto(0, 0, 0, 0, 0, 0), [], new AudienceInsightsDto(0, [], []));

            var downloads = await _db.Downloads.AsNoTracking()
                .Where(d => bookIds.Contains(d.BookID))
                .Select(d => new { d.BookID, d.UserID, d.IsFinishReading })
                .ToListAsync();

            var favorites = await _db.FavoriteBooks.AsNoTracking()
                .Where(f => bookIds.Contains(f.BookId))
                .CountAsync();

            var ratings = await _db.BookRatings.AsNoTracking()
                .Where(r => bookIds.Contains(r.BookId))
                .Select(r => new { r.Rating, r.Comment, r.UserId, r.BookId })
                .ToListAsync();

            var favsByBook = await _db.FavoriteBooks.AsNoTracking()
                .Where(f => bookIds.Contains(f.BookId))
                .GroupBy(f => f.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BookId, x => x.Count);

            var ratingStatsByBook = ratings
                .GroupBy(r => r.UserId != null ? r.BookId : r.BookId)
                .ToDictionary(g => g.Key, g => (Count: g.Count(), Avg: g.Average(x => (double)x.Rating)));

            var dist = new RatingDistributionDto(
                FiveStars: ratings.Count(r => Math.Round(r.Rating) == 5),
                FourStars: ratings.Count(r => Math.Round(r.Rating) == 4),
                ThreeStars: ratings.Count(r => Math.Round(r.Rating) == 3),
                TwoStars: ratings.Count(r => Math.Round(r.Rating) == 2),
                OneStar: ratings.Count(r => Math.Round(r.Rating) == 1),
                Total: ratings.Count
            );

            var performance = books.Select(b =>
            {
                var bd = downloads.Where(d => d.BookID == b.Id).ToList();
                ratingStatsByBook.TryGetValue(b.Id, out var rs);
                return new AuthorBookPerformanceDto(
                    BookId: b.Id,
                    Title: b.Title,
                    Downloads: bd.Count,
                    FinishedReads: bd.Count(d => d.IsFinishReading),
                    Favorites: favsByBook.GetValueOrDefault(b.Id),
                    RatingCount: rs.Count,
                    AvgRating: rs.Count == 0 ? 0 : Math.Round(rs.Avg, 2)
                );
            })
            .OrderByDescending(p => p.Downloads + p.Favorites + p.RatingCount)
            .ToList();

            // Audience insights
            var readerIds = downloads.Select(d => d.UserID).Where(id => id != null).Distinct().ToList();
            var interestedCategories = new List<CategoryShareDto>();
            foreach (var reader in readerIds.Take(200)) // cap cost
            {
                foreach (var share in await GetCategorySharesAsync(reader))
                {
                    var existing = interestedCategories.FirstOrDefault(c => c.CategoryId == share.CategoryId);
                    if (existing == null) interestedCategories.Add(share);
                    else
                        interestedCategories[interestedCategories.IndexOf(existing)] =
                            existing with { Percent = existing.Percent + share.Percent };
                }
            }
            var totalInterest = interestedCategories.Sum(c => c.Percent);
            if (totalInterest > 0)
                interestedCategories = interestedCategories
                    .OrderByDescending(c => c.Percent)
                    .Select(c => c with { Percent = Math.Round(c.Percent / totalInterest * 100, 1) })
                    .ToList();

            var popularContent = performance.Take(5).Select(p => p.Title).ToList();

            return new AuthorDashboardDto(
                PublishedBooks: books.Count,
                TotalDownloads: downloads.Count,
                TotalFinishedReads: downloads.Count(d => d.IsFinishReading),
                TotalFavorites: favorites,
                AvgRating: ratings.Count == 0 ? 0 : Math.Round(ratings.Average(r => (double)r.Rating), 2),
                RatingCount: ratings.Count,
                RatingDistribution: dist,
                BookPerformance: performance,
                Audience: new AudienceInsightsDto(readerIds.Count, interestedCategories, popularContent)
            );
        }

        // ------------------------------------------------------------------
        // Admin overview & growth
        // ------------------------------------------------------------------

        public async Task<PlatformOverviewDto> GetPlatformOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var prevStart = monthStart.AddMonths(-1);

            long totalUsers = await _db.Users.CountAsync();
            long newUsers = await _db.Users.CountAsync(u => u.JoinedAt >= monthStart);
            long prevUsers = await _db.Users.CountAsync(u => u.JoinedAt >= prevStart && u.JoinedAt < monthStart);
            long totalAuthors = await _db.Authors.CountAsync();
            long newAuthors = await _db.Authors.CountAsync(a => a.IsRegistered);
            long totalBooks = await _db.Books.CountAsync();

            long totalDownloads = await _db.Downloads.CountAsync();
            long downloadsThisMonth = await _db.Downloads.CountAsync(d => d.DownloadedAt >= monthStart);
            long downloadsPrevMonth = await _db.Downloads.CountAsync(d => d.DownloadedAt >= prevStart && d.DownloadedAt < monthStart);

            long totalInteractions = await _db.Downloads.CountAsync()
                + await _db.FavoriteBooks.CountAsync()
                + await _db.BookRatings.CountAsync();
            long interactionsNow = await _db.Downloads.CountAsync(d => d.DownloadedAt >= monthStart)
                + await _db.FavoriteBooks.CountAsync()
                + await _db.BookRatings.CountAsync(r => r.CreatedAt >= monthStart);
            long interactionsPrev = await _db.Downloads.CountAsync(d => d.DownloadedAt >= prevStart && d.DownloadedAt < monthStart)
                + await _db.BookRatings.CountAsync(r => r.CreatedAt >= prevStart && r.CreatedAt < monthStart);

            long totalRatings = await _db.BookRatings.CountAsync();

            return new PlatformOverviewDto(
                TotalUsers: totalUsers,
                NewUsersThisMonth: newUsers,
                UserGrowthPercent: PercentChange(prevUsers, newUsers),
                TotalAuthors: totalAuthors,
                NewAuthorsThisMonth: newAuthors,
                TotalBooks: totalBooks,
                TotalDownloads: totalDownloads,
                TotalRatings: totalRatings,
                TotalInteractions: totalInteractions,
                DownloadsGrowth: new MonthlyGrowthDto(downloadsThisMonth, downloadsPrevMonth, PercentChange(downloadsPrevMonth, downloadsThisMonth)),
                InteractionsGrowth: new MonthlyGrowthDto(interactionsNow, interactionsPrev, PercentChange(interactionsPrev, interactionsNow))
            );
        }

        public async Task<PlatformGrowthDto> GetPlatformGrowthAsync(int months = 12)
        {
            months = Math.Clamp(months, 1, 24);
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-(months - 1));

            var downloads = await _db.Downloads.AsNoTracking()
                .Where(d => d.DownloadedAt >= start)
                .Select(d => d.DownloadedAt).ToListAsync();
            var ratings = await _db.BookRatings.AsNoTracking()
                .Where(r => r.CreatedAt >= start)
                .Select(r => r.CreatedAt).ToListAsync();
            var joined = await _db.Users.AsNoTracking()
                .Where(u => u.JoinedAt >= start)
                .Select(u => u.JoinedAt).ToListAsync();
            var uploads = await _db.Books.AsNoTracking()
                .Where(b => b.UploadedAt >= start)
                .Select(b => b.UploadedAt).ToListAsync();

            var periods = Enumerable.Range(0, months)
                .Select(i => start.AddMonths(i))
                .ToList();

            List<TimeSeriesPointDto> Series(List<DateTime> dates) =>
                periods.Select(p => new TimeSeriesPointDto(
                    Period: p.ToString("yyyy-MM"),
                    Count: dates.Count(d => d.Year == p.Year && d.Month == p.Month)
                )).ToList();

            return new PlatformGrowthDto(Series(joined), Series(uploads), Series(downloads), Series(ratings));
        }

        // ------------------------------------------------------------------
        // Engagement
        // ------------------------------------------------------------------

        public async Task<List<ActiveUserDto>> GetMostActiveUsersAsync(int top = 5)
        {
            var (wRead, wLike, wRating, wComment) = Weights;
            var features = await GetUserFeaturesAsync();
            return features
                .Select(f => new ActiveUserDto(
                    f.UserId, f.UserName, f.Downloads, f.Favorites, f.Ratings, f.Comments,
                    Math.Round(f.EngagementScore(wRead, wLike, wRating, wComment), 1)))
                .OrderByDescending(u => u.EngagementScore)
                .Take(top)
                .ToList();
        }

        // ------------------------------------------------------------------
        // Books
        // ------------------------------------------------------------------

        public async Task<TopBooksDto> GetTopBooksAsync(int top = 5)
        {
            var titles = await BookTitlesMapAsync();

            List<TopBookDto> Rank(List<KeyValuePair<string, long>> pairs, double? avg = null, int count = 0) =>
                pairs.OrderByDescending(p => p.Value).Take(top)
                    .Select(p => new TopBookDto(p.Key, titles.GetValueOrDefault(p.Key, p.Key), p.Value,
                        avg.HasValue ? Math.Round(avg.Value, 2) : null, count > 0 ? count : null))
                    .ToList();

            var mostDownloaded = await _db.Downloads.AsNoTracking()
                .GroupBy(d => d.BookID)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync();
            var mostFinished = await _db.Downloads.AsNoTracking()
                .Where(d => d.IsFinishReading)
                .GroupBy(d => d.BookID)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync();
            var mostFavorited = await _db.FavoriteBooks.AsNoTracking()
                .GroupBy(f => f.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync();
            var ratingStats = await _db.BookRatings.AsNoTracking()
                .GroupBy(r => r.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count(), Avg = g.Average(x => (double)x.Rating) })
                .ToListAsync();

            return new TopBooksDto(
                MostDownloaded: Rank(mostDownloaded.Select(x => new KeyValuePair<string, long>(x.BookId, x.Count)).ToList()),
                MostRead: Rank(mostFinished.Select(x => new KeyValuePair<string, long>(x.BookId, x.Count)).ToList()),
                MostFavorited: Rank(mostFavorited.Select(x => new KeyValuePair<string, long>(x.BookId, x.Count)).ToList()),
                HighestRated: ratingStats
                    .OrderByDescending(x => x.Avg)
                    .ThenByDescending(x => x.Count) // avoid single-rating books dominating
                    .Take(top)
                    .Select(x => new TopBookDto(x.BookId, titles.GetValueOrDefault(x.BookId, x.BookId), x.Count, Math.Round(x.Avg, 2), x.Count))
                    .ToList()
            );
        }

        public async Task<List<AssociationRuleDto>> GetBookAssociationRulesAsync(double minSupport = 0.02, double minConfidence = 0.3)
        {
            var (transactions, labels, _) = await BuildBookTransactionsAsync();
            return FpGrowthService.Mine(transactions, labels, minSupport, minConfidence);
        }

        public async Task<List<CategoryAssociationDto>> GetCategoryAssociationRulesAsync(double minSupport = 0.02, double minConfidence = 0.3)
        {
            var transactions = await BuildUserCategorySetsAsync();
            var labels = await _db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name);
            var rules = FpGrowthService.Mine(transactions, labels, minSupport, minConfidence);

            return rules.Select(r => new CategoryAssociationDto(r.AntecedentTitle, r.ConsequentTitle, r.Support, r.Confidence, r.Lift)).ToList();
        }

        // ------------------------------------------------------------------
        // Segmentation (K-Means)
        // ------------------------------------------------------------------

        public async Task<SegmentationResultDto> GetUserSegmentsAsync(int k = 3)
        {
            var features = await GetUserFeaturesAsync();

            // Need at least k*2 users with any activity to cluster meaningfully.
            if (features.Count < k * 2 || features.All(f => f.Downloads + f.Favorites + f.Ratings == 0))
                return new SegmentationResultDto(0, [], []);

            var vectors = features.Select(f => Normalize(f)).ToArray();

            var assignments = KMeansService.Fit(vectors, k);

            // Order clusters by centroid magnitude -> Casual / Active / Enthusiast
            var clusterStrength = assignments
                .Select((c, i) => (cluster: c, index: i))
                .GroupBy(x => x.cluster)
                .OrderBy(g => vectors.Where((v, i) => assignments[i] == g.Key).Average(v => v.Sum()))
                .Select(g => g.Key)
                .ToList();

            string LabelFor(int cluster) => clusterStrength switch
            {
                var list when list.Count > 2 && list.IndexOf(cluster) == 2 => "Book Enthusiast",
                var list when list.Count > 1 && list.IndexOf(cluster) == 1 => "Active Reader",
                var list when list.IndexOf(cluster) == 0 => "Casual Reader",
                _ => $"Segment {cluster}"
            };

            var summaries = assignments
                .Select((c, i) => (cluster: c, feature: features[i]))
                .GroupBy(x => x.cluster)
                .Select(g => new ClusterSummaryDto(
                    Label: LabelFor(g.Key),
                    UserCount: g.Count(),
                    Percentage: Math.Round((double)g.Count() / features.Count * 100, 1),
                    AvgDownloads: Math.Round(g.Average(x => x.feature.Downloads), 2),
                    AvgFinishedReads: Math.Round(g.Average(x => x.feature.FinishedReads), 2),
                    AvgFavorites: Math.Round(g.Average(x => x.feature.Favorites), 2),
                    AvgRatings: Math.Round(g.Average(x => x.feature.Ratings), 2),
                    AvgComments: Math.Round(g.Average(x => x.feature.Comments), 2)
                ))
                .OrderBy(s => s.Label switch
                {
                    "Casual Reader" => 0,
                    "Active Reader" => 1,
                    "Book Enthusiast" => 2,
                    _ => 3
                })
                .ToList();

            var userSegments = features
                .Select((f, i) => (f.UserId, LabelFor(assignments[i])))
                .ToDictionary(x => x.Item1, x => x.Item2);

            return new SegmentationResultDto(k, summaries, userSegments);
        }

        // ------------------------------------------------------------------
        // Anomaly detection (robust z-score over engagement mix)
        // ------------------------------------------------------------------

        public async Task<List<AnomalousUserDto>> DetectAnomaliesAsync(double threshold = 3.0, int top = 10)
        {
            var features = await GetUserFeaturesAsync();
            if (features.Count < 10) return [];

            // Feature matrix columns: Downloads, FinishedReads, Favorites, Ratings, Comments
            double[][] Matrix() => features
                .Select(f => new[] { (double)f.Downloads, (double)f.FinishedReads, (double)f.Favorites, (double)f.Ratings, (double)f.Comments })
                .ToArray();

            var matrix = Matrix();
            int dim = matrix[0].Length;

            // Robust scaling using median and MAD
            var medians = new double[dim];
            var mads = new double[dim];
            for (int d = 0; d < dim; d++)
            {
                var col = matrix.Select(r => r[d]).OrderBy(x => x).ToArray();
                medians[d] = Percentile(col, 0.5);
                var deviations = col.Select(x => Math.Abs(x - medians[d])).OrderBy(x => x).ToArray();
                mads[d] = Percentile(deviations, 0.5);
                if (mads[d] == 0) mads[d] = 1e-9;
            }

            var scores = new List<(UserFeatureRow User, double Score)>();
            foreach (var f in features)
            {
                double[] vec = [f.Downloads, f.FinishedReads, f.Favorites, f.Ratings, f.Comments];
                double score = 0;
                for (int d = 0; d < dim; d++)
                    score = Math.Max(score, Math.Abs(vec[d] - medians[d]) / mads[d]);
                scores.Add((f, score));
            }

            return scores
                .Where(s => s.Score >= threshold)
                .OrderByDescending(s => s.Score)
                .Take(top)
                .Select(s => new AnomalousUserDto(
                    s.User.UserId, s.User.UserName,
                    s.User.Downloads, s.User.FinishedReads, s.User.Favorites,
                    s.User.Ratings, s.User.Comments,
                    Math.Round(s.Score, 2)))
                .ToList();
        }

        // ------------------------------------------------------------------
        // Authors
        // ------------------------------------------------------------------

        public async Task<TopAuthorsDto> GetTopAuthorsAsync(int top = 5)
        {
            var authors = await _db.Authors.AsNoTracking().ToListAsync();

            var publishedBooks = await _db.BookAuthors.AsNoTracking()
                .GroupBy(ba => ba.Author_Id)
                .Select(g => new { AuthorId = g.Key, Count = g.Count() })
                .ToListAsync();

            var readsByAuthor = await _db.Downloads.AsNoTracking()
                .Join(_db.BookAuthors, d => d.BookID, ba => ba.Book_Id, (d, ba) => new { ba.Author_Id })
                .GroupBy(x => x.Author_Id)
                .Select(g => new { AuthorId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Engagement: downloads + favorites + ratings across the author's books.
            var bookAuthorPairs = await _db.BookAuthors.AsNoTracking()
                .Select(ba => new { ba.Author_Id, ba.Book_Id }).ToListAsync();
            var bookToAuthors = bookAuthorPairs.GroupBy(p => p.Book_Id)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Author_Id).ToList());

            var favCounts = await _db.FavoriteBooks.AsNoTracking()
                .GroupBy(f => f.BookId).Select(g => new { g.Key, C = g.Count() }).ToListAsync();
            var ratingCounts = await _db.BookRatings.AsNoTracking()
                .GroupBy(r => r.BookId).Select(g => new { g.Key, C = g.Count() }).ToListAsync();
            var downloadCounts = await _db.Downloads.AsNoTracking()
                .GroupBy(d => d.BookID).Select(g => new { g.Key, C = g.Count() }).ToListAsync();

            var engagement = new Dictionary<string, long>();
            void AddEngagement(string bookId, long amount)
            {
                if (!bookToAuthors.TryGetValue(bookId, out var authorIds)) return;
                foreach (var a in authorIds)
                    engagement[a] = engagement.GetValueOrDefault(a) + amount;
            }
            foreach (var x in downloadCounts) AddEngagement(x.Key, x.C);
            foreach (var x in favCounts) AddEngagement(x.Key, x.C);
            foreach (var x in ratingCounts) AddEngagement(x.Key, x.C);

            var nameById = authors.ToDictionary(a => a.Id, a => a.Name);

            List<AuthorStatDto> StatList(IEnumerable<(string Id, long Value)> rows) =>
                rows.OrderByDescending(r => r.Value).Take(top)
                    .Select(r => new AuthorStatDto(r.Id, nameById.GetValueOrDefault(r.Id, r.Id), r.Value, 0))
                    .ToList();

            var engagementStats = StatList(engagement.Select(kv => (kv.Key, kv.Value)));

            // Attach normalized engagement score onto all lists' entries where known
            double maxEngagement = Math.Max(1, engagement.Values.DefaultIfEmpty(0).Max());

            List<AuthorStatDto> WithScore(List<AuthorStatDto> list) => list
                .Select(a => a with { EngagementScore = Math.Round(engagement.GetValueOrDefault(a.AuthorId) / maxEngagement * 100, 1) })
                .ToList();

            return new TopAuthorsDto(
                ByPublishedBooks: WithScore(StatList(publishedBooks.Select(x => (x.AuthorId, (long)x.Count)))),
                ByReads: WithScore(StatList(readsByAuthor.Select(x => (x.AuthorId, (long)x.Count)))),
                ByEngagement: WithScore(engagementStats)
            );
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Builds per-user transaction sets of downloaded/read book ids plus a book-id -> title map.
        /// </summary>
        private async Task<(List<HashSet<string>> Transactions, Dictionary<string, string> Labels, Dictionary<string, HashSet<string>> UserTransactions)>
            BuildBookTransactionsAsync()
        {
            var rows = await _db.Downloads.AsNoTracking()
                .Select(d => new { d.UserID, d.BookID })
                .ToListAsync();

            var labels = await BookTitlesMapAsync();

            var userTransactions = rows
                .GroupBy(r => r.UserID)
                .ToDictionary(g => g.Key, g => g.Select(r => r.BookID).ToHashSet());

            return (userTransactions.Values.ToList(), labels, userTransactions);
        }

        private async Task<Dictionary<string, string>> BookTitlesMapAsync() =>
            await _db.Books.AsNoTracking().ToDictionaryAsync(b => b.Id, b => b.Title);

        /// <summary>Per-user sets of categories derived from their interactions.</summary>
        private async Task<List<HashSet<string>>> BuildUserCategorySetsAsync()
        {
            var rows = await (
                from d in _db.Downloads.AsNoTracking()
                join bc in _db.BookCategories.AsNoTracking() on d.BookID equals bc.bookId
                select new { d.UserID, bc.categoryId }
            ).Distinct().ToListAsync();

            return rows
                .GroupBy(r => r.UserID)
                .Select(g => g.Select(r => r.categoryId).ToHashSet())
                .ToList();
        }

        private static double[] Normalize(UserFeatureRow f)
        {
            // Log-scaling dampens heavy tails so clustering isn't dominated by outliers.
            double L(double v) => Math.Log10(1 + v);
            return [L(f.Downloads), L(f.FinishedReads), L(f.Favorites), L(f.Ratings), L(f.Comments)];
        }

        private static double Percentile(double[] sorted, double p)
        {
            if (sorted.Length == 0) return 0;
            var idx = (sorted.Length - 1) * p;
            int lo = (int)Math.Floor(idx);
            int hi = (int)Math.Ceiling(idx);
            return sorted[lo] + (sorted[hi] - sorted[lo]) * (idx - lo);
        }

        private static double PercentChange(long previous, long current) =>
            previous == 0 ? (current == 0 ? 0 : 100) : Math.Round((double)(current - previous) / previous * 100, 1);
    }

    internal static class EnumerableExtensions
    {
        public static int FirstIndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (predicate(item)) return i;
                i++;
            }
            return -1;
        }
    }
}
