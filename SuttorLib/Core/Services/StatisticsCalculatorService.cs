using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SuttorLib.Core.Interfaces;
using SuttorLib.Data;
using SuttorLibrary.Models.Analytics;
using Microsoft.Extensions.Options;
using SuttorLibrary.Data;

namespace SuttorLib.Core.Services
{
    public class StatisticsCalculatorService : IStatisticsCalculator
    {
        private readonly AppDbContext _sqlContext;
        private readonly IMongoCollection<PlatformStatistic> _statisticsCollection;

        public StatisticsCalculatorService(AppDbContext sqlContext, IOptions<BlogDbSettings> mongoSettings)
        {
            _sqlContext = sqlContext;
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _statisticsCollection = mongoDatabase.GetCollection<PlatformStatistic>("PlatformStatistics");
        }

        public async Task CalculateDailyStatisticsAsync()
        {
            var totalUsers = await _sqlContext.Users.CountAsync();

            var stats = new List<PlatformStatistic>
            {
                new PlatformStatistic
                {
                    MetricName = "TotalUsers",
                    Value = totalUsers,
                    CalculationDate = DateTime.UtcNow
                }
            };

            if (stats.Any())
            {
                await _statisticsCollection.InsertManyAsync(stats);
            }
        }

        public async Task<IEnumerable<PlatformStatistic>> GetLatestStatisticsAsync()
        {
            return await _statisticsCollection.Find(_ => true)
                .SortByDescending(s => s.CalculationDate)
                .Limit(50)
                .ToListAsync();
        }
    }
}