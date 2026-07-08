using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLibrary.Models.Analytics
{
    public class PlatformStatistic
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("MetricName")]
        public string MetricName { get; set; } = string.Empty;

        [BsonElement("Value")]
        public double Value { get; set; }

        [BsonElement("CalculationDate")]
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
    }

    public class UserRecommendation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("RecommendedItemIds")]
        public List<string> RecommendedItemIds { get; set; } = new();

        [BsonElement("ItemType")] // Book, Blog, etc.
        public string ItemType { get; set; } = string.Empty;

        [BsonElement("AlgorithmType")]
        public string AlgorithmType { get; set; } = string.Empty;

        [BsonElement("ConfidenceScore")]
        public double ConfidenceScore { get; set; }

        [BsonElement("GeneratedAt")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}