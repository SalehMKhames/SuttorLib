using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLib.Models;

public class Message
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("role")]
    public Role role { get; set; }
    
    [BsonElement("Content")]
    [BsonRepresentation(BsonType.String)]
    public string Content { get; set; }
    
    [BsonElement("timestamp")]
    public DateTime timestamp { get; set; } 
}

public enum Role
{
    User,
    Assistant
}