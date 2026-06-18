using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SuttorLib.Models
{
    public class Blog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [Required]
        [BsonElement("title")]
        [BsonRepresentation(BsonType.String)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [BsonElement("content")]
        [BsonRepresentation(BsonType.String)]
        public string Content { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [BsonElement("publisher_Id")]
        public string PublisherId { get; set; } = string.Empty;

        [BsonElement("likes")]
        public int Likes { get; set; }

        [BsonElement("dislikes")]
        public int Dislikes { get; set; }

        [BsonElement("views")]
        public int Views { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("userLikes")]
        [BsonRepresentation(BsonType.Array)]
        public List<string> UserIdsLikes { get; set; } = new();
        
        [BsonElement("userDislikes")]
        [BsonRepresentation(BsonType.Array)]
        public List<string> UserIdsDislikes { get; set; } = new();

        [BsonElement("isPublished")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsPublished { get; set; }

        [BsonElement("comments")]
        [BsonRepresentation(BsonType.Array)]
        public List<ObjectId> Comments { get; set; } = new();

        [BsonElement("category")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = string.Empty;
    }
}
