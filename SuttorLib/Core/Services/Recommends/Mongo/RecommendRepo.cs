using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SuttorLib.Data;
using SuttorLib.Models.Recommender;

namespace SuttorLib.Core.Services.Recommends.Mongo
{
    public class RecommendRepo : IRecommendRepo
    {
        private readonly IMongoCollection<BookRecommendationDocument> _recommend;
        private readonly IOptions<StatisticsSettings> _dbSettings;

        public RecommendRepo(IOptions<StatisticsSettings> dbSettings)
        {
            _dbSettings = dbSettings;

            var cs = _dbSettings?.Value?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("StatisticsDbSettings.ConnectionString is missing. Ensure configuration binds the 'StatisticsDbSettings' section.");

            var mongoClient = new MongoClient(cs);
            var mongoDatabase = mongoClient.GetDatabase(_dbSettings?.Value?.DatabaseName);

            _recommend = mongoDatabase.GetCollection<BookRecommendationDocument>(_dbSettings?.Value?.RecommendationsCollection);
        }

        public async Task<BookRecommendationDocument?> GetForUserAsync(string userId)
        {
            var filter = Builders<BookRecommendationDocument>.Filter.Eq(d => d.UserId, userId);
            return await _recommend.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpsertAsync(BookRecommendationDocument document)
        {
            var filter = Builders<BookRecommendationDocument>.Filter.Eq(d => d.UserId, document.UserId);
            var update = Builders<BookRecommendationDocument>.Update.Set(d => d.RecommendedBooks, document.RecommendedBooks)
                .Set(d => d.Source, document.Source)
                .Set(d => d.GeneratedAtUtc, document.GeneratedAtUtc);

            await _recommend.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }
    }
}
