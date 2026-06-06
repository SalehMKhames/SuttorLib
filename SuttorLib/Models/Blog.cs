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

        [BsonElement("content")]
        [BsonRepresentation(BsonType.String)]
        public string Content { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
        public List<string> UserLikes { get; set; } = new();
        
        [BsonElement("userDislikes")]
        public List<string> UserDislikes { get; set; } = new();

        [BsonElement("comments")]
        public List<Comment> Comments { get; set; } = new();

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;
    }
}
