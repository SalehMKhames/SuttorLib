using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLib.Models.Recommender
{
    /// <summary>
    /// One document per user: their current top-N recommended books, as
    /// computed by the last pipeline run. This is what the API reads from —
    /// recommendations are precomputed and cached here, not calculated live
    /// on every request.
    /// </summary>
    public class BookRecommendationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public List<RecommendedBook> RecommendedBooks { get; set; } = new();

        // "MatrixFactorization" or "InterestFallback" — useful for the API/UI
        // to distinguish "personalized for you" from "popular in your interests".
        public string Source { get; set; } = string.Empty;

        public DateTime GeneratedAtUtc { get; set; }
    }

    public class RecommendedBook
    {
        public string BookId { get; set; }
        public float Score { get; set; }
    }
}
