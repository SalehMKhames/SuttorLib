using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SuttorLib.Core.Interfaces;
using SuttorLib.Data;
using SuttorLibrary.Models.Analytics;

namespace SuttorLib.Core.Services.Statistics
{
    public class RecommendationEngineService : IRecommendationEngine
    {
        private readonly IMongoCollection<UserRecommendation> _recommendationsCollection;
        private readonly HttpClient _httpClient;
        private readonly string _pythonAiApiUrl;

        public RecommendationEngineService(
            IOptions<BlogDbSettings> mongoSettings,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _recommendationsCollection = mongoDatabase.GetCollection<UserRecommendation>("RecommendationsCollection");

            _httpClient = httpClient;
            _pythonAiApiUrl = configuration.GetValue<string>("PythonAiApiUrl") ?? "http://localhost:8000/recommendations";
        }

        public async Task GenerateUserRecommendationsAsync()
        {
            // Placeholder: Call Python API with appropriate data (e.g. Interaction histories)
            // Python API evaluates logic and returns computed recommendations

            // var interactionPayload = new { /* Populate with user interaction data */ };

            // var response = await _httpClient.PostAsJsonAsync(_pythonAiApiUrl, interactionPayload);
            // var results = await response.Content.ReadFromJsonAsync<List<UserRecommendation>>();
            // if(results != null) await _recommendationsCollection.InsertManyAsync(results);

            await Task.CompletedTask; // Stub for demonstration
        }

        public async Task<IEnumerable<UserRecommendation>> GetRecommendationsForUserAsync(string userId, string itemType)
        {
            var filter = Builders<UserRecommendation>.Filter.And(
                Builders<UserRecommendation>.Filter.Eq(r => r.UserId, userId),
                Builders<UserRecommendation>.Filter.Eq(r => r.ItemType, itemType)
            );

            return await _recommendationsCollection.Find(filter)
                .SortByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }
    }
}