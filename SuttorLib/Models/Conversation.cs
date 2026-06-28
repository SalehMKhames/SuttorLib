using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuttorLib.Models;

public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonRepresentation(BsonType.String)]
    [BsonElement("createdAt")]
    public DateTime CreatedAT { get; set; }

    [BsonRepresentation(BsonType.String)]
    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("messageIds")]
    public List<ObjectId> MessageIds { get; set; } = new();
    
}