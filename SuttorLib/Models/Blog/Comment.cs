using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLib.Models.Blog
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("Tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("likes")]
        public int Likes { get; set; }

        [BsonElement("dislikes")]
        public int Dislikes { get; set; }

        [BsonElement("userLikes")]
        public List<string> UserIdsLikes { get; set; } = new();

        [BsonElement("userDislikes")]
        public List<string> UserIdsDislikes { get; set; } = new();

        [BsonElement("replies")]
        public List<Comment> Replies { get; set; } = new();
    }
}
