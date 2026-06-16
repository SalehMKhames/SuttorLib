using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLib.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("content")]
        [BsonRepresentation(BsonType.String)]
        public string Content { get; set; } = string.Empty;

        [BsonElement("commenter")]
        [BsonRepresentation(BsonType.String)]
        public string CommenterId { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("Tags")]
        [BsonRepresentation(BsonType.Array)]
        public List<string> Tags { get; set; } = new();

        [BsonElement("likes")]
        public int Likes { get; set; }

        [BsonElement("dislikes")]
        public int Dislikes { get; set; }

        [BsonElement("userLikes")]
        public List<string> UserLikes { get; set; } = new();

        [BsonElement("userDislikes")]
        public List<string> UserDislikes { get; set; } = new();

        [BsonElement("replies")]
        public List<Comment> Replies { get; set; } = new();
    }
}
