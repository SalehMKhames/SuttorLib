using SuttorLib.DTOs;
using SuttorLib.Models;

namespace SuttorLibrary.Core.Services.aiConversations;

public interface IConversationService
{
    public Task<Conversation> CreateConversation(AddConversationDTO conversationDTO, string userId);
    public Task<List<ConversationDTO>> GetConversationsAsync(string userId);
    public Task<ConversationDTO> GetConversation(string id, string userId);
    public Task<Conversation> UpdateConversation(string ConvId, UpdateConversationDTO conversationDTO, string userId);
    public Task<bool> DeleteConversation(string id, string userId);
    public Task DeleteAllConversations(string userId);

    public Task<Message> CreateMessage(AddMessageDTO messageDTO);
    public Task<List<Message>?> GetMessagesForConversation(string conversationID);

    Task<bool> AddMessageToConversationAsync(string conversationId, string messageId);

}