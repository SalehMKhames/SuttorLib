using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SuttorLib.Data;
using SuttorLib.DTOs;
using SuttorLib.Models;

namespace SuttorLibrary.Core.Services.aiConversations;

public class ConversationService : IConversationService
{
    private readonly IMongoCollection<Conversation> _conversation;
    private readonly IMongoCollection<Message> _message;
    private readonly IOptions<AiDbSettings> _dbSettings;
    private readonly IHttpContextAccessor _httpContext;

    public ConversationService(IOptions<AiDbSettings> dbSettings, IHttpContextAccessor httpContext)
    {
        _dbSettings = dbSettings;
        _httpContext = httpContext;

        var cs = _dbSettings?.Value?.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("AiDbSettings.ConnectionString is missing. Ensure configuration binds the 'AiDbSettings' section.");

        var mongoClient = new MongoClient(cs);
        var mongoDatabase = mongoClient.GetDatabase(_dbSettings?.Value?.DatabaseName);

        _conversation = mongoDatabase.GetCollection<Conversation>(_dbSettings?.Value?.Conversations);
        _message = mongoDatabase.GetCollection<Message>(_dbSettings?.Value?.Messages);
    }


    public async Task<Conversation> CreateConversation(AddConversationDTO createDto, string userId)
    {
        List<ObjectId> messageIds = new();
        if (createDto.Messages?.Count > 0)
            foreach (var mess in createDto.Messages)
            {
                var m = await CreateMessage(mess);
                messageIds.Add(m.Id);
            }
        

        var conversation = new Conversation
        {
            UserId = userId,
            Title = createDto.Title ?? "Untitled Conversation",
            CreatedAT = DateTime.UtcNow,
            MessageIds = messageIds
        };

        await _conversation.InsertOneAsync(conversation);

        conversation = await _conversation.Find(c => c.Id == conversation.Id).FirstOrDefaultAsync();

        return conversation;
    }

    public async Task<ConversationDTO> GetConversation(string id, string userId)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new KeyNotFoundException("Conversations not found");

        var conversation = await _conversation
            .Find(c => c.Id == objectId)
            .FirstOrDefaultAsync();

        if (conversation == null)
            throw new KeyNotFoundException("Conversation not found");

        if (conversation.UserId is null || string.IsNullOrEmpty(conversation.UserId))
            throw new UnauthorizedAccessException();

        List<Message> messages = await GetMessagesForConversation(id.ToString()) ?? [];  


        return new ConversationDTO
        {
            Id = conversation.Id.ToString(),
            Title = conversation.Title ?? "No Title",
            CreatedAT = conversation.CreatedAT,
            Messages = messages
        };
    }
    
    public async Task<List<ConversationDTO>> GetConversationsAsync(string userId)
    {
        var conversations = await _conversation
        .Find(c => c.UserId == userId)
        .ToListAsync();

        if (conversations is null)
            throw new KeyNotFoundException();

        var conversationDtos = new List<ConversationDTO>();

        foreach (var conversation in conversations)
        {
            var messages = await _message
                .Find(m => conversation.MessageIds.Contains(m.Id))
                .ToListAsync();

            conversationDtos.Add(new ConversationDTO
            {
                Id = conversation.Id.ToString(),
                Title = conversation.Title ?? "",
                CreatedAT = conversation.CreatedAT,
                Messages = messages
            });
        }

        return conversationDtos;
    }

    public async Task<Conversation> UpdateConversation(string convId, UpdateConversationDTO conversationDTO, string userid)
    {
        if (!ObjectId.TryParse(convId, out var objectId))
            throw new KeyNotFoundException("Conversation not found");

        var conv = await _conversation.Find(c => c.Id == objectId).FirstOrDefaultAsync();
        if (conv.UserId != userid)
            throw new UnauthorizedAccessException();

        // Insert or update messages
        List<ObjectId> messageIds = new();
        if (conversationDTO.Messages?.Count > 0)
        {
            foreach (var message in conversationDTO.Messages)
            {
                if (message.Id == ObjectId.Empty)
                {
                    // New message, insert it
                    await _message.InsertOneAsync(message);
                }
                else
                {
                    // Existing message, update it
                    var filter = Builders<Message>.Filter.Eq(m => m.Id, message.Id);
                    await _message.ReplaceOneAsync(filter, message);
                }
                messageIds.Add(message.Id);
            }
        }

        var updateDefinition = Builders<Conversation>.Update
            .Set(c => c.Title, conversationDTO.Title);

        if (messageIds.Count > 0)
        {
            updateDefinition = updateDefinition.Set(c => c.MessageIds, messageIds);
        }

        var result = await _conversation.UpdateOneAsync(
            c => c.Id == objectId,
            updateDefinition);

        conv = await _conversation.Find(c => c.Id == objectId).FirstOrDefaultAsync();
        return conv;
    }

    public async Task<bool> DeleteConversation(string id, string userId)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException("Invalid conversation ID");

        var conversation = await _conversation
            .Find(c => c.Id == objectId)
            .FirstOrDefaultAsync();

        if (conversation is null)
            throw new KeyNotFoundException("Conversation not found");

        if (conversation.UserId != userId)
            throw new UnauthorizedAccessException();

        // Delete all associated messages
        if (conversation.MessageIds.Count > 0)
        {
            await _message.DeleteManyAsync(
                m => conversation.MessageIds.Contains(m.Id));
        }

        // Delete the conversation
        var result = await _conversation.DeleteOneAsync(c => c.Id == objectId);

        return result.IsAcknowledged;
    }

    public async Task DeleteAllConversations(string userId)
    {
        if (!Guid.TryParse(userId, out var userObjectId))
            throw new UnauthorizedAccessException();

        var conversations = await _conversation
            .Find(c => c.UserId == userId)
            .ToListAsync();

        foreach (var conversation in conversations)
        {
            // Delete all associated messages
            if (conversation.MessageIds.Count > 0)
            {
                await _message.DeleteManyAsync(
                    m => conversation.MessageIds.Contains(m.Id));
            }

            // Delete the conversation
            await _conversation.DeleteOneAsync(c => c.Id == conversation.Id);
        }
    }

    public async Task<bool> AddMessageToConversationAsync(string conversationId, string messageId)
    {
        if (!ObjectId.TryParse(conversationId, out var convObjectId) ||
            !ObjectId.TryParse(messageId, out var msgObjectId))
            return false;

        var result = await _conversation.UpdateOneAsync(
            c => c.Id == convObjectId,
            Builders<Conversation>.Update.AddToSet(c => c.MessageIds, msgObjectId));

        return result.ModifiedCount > 0;
    }


    // ==========================================================================================

    public async Task<Message> CreateMessage(AddMessageDTO messageDTO)
    {
        if(messageDTO.Content is null || string.IsNullOrEmpty(messageDTO.Content))
            throw new ArgumentException("Message content cannot be null or empty.");

        var message = new Message
        {
            Id = ObjectId.GenerateNewId(),
            role = messageDTO.role,
            Content = messageDTO.Content,
            timestamp = DateTime.UtcNow
        };

        await _message.InsertOneAsync(message);
        return message;
    }

    public async Task<List<Message>?> GetMessagesForConversation(string conversationID)
    {
        if (!ObjectId.TryParse(conversationID, out var objectId))
            throw new KeyNotFoundException("Conversation not found");

        var con = await _conversation.Find(c => c.Id == objectId).FirstOrDefaultAsync();
        if (con is null)
            throw new KeyNotFoundException($"Conversation with id: {conversationID} not found");

        var messages = new List<Message>();
        foreach (var message in con.MessageIds)
        {
            var m = await _message.Find(m => m.Id == message).FirstOrDefaultAsync();
            messages.Add(m);
        }

        return messages;
    }
}