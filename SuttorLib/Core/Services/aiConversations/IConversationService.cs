using SuttorLib.DTOs;

namespace SuttorLibrary.Core.Services.aiConversations;

public interface IConversation
{
    public Task CreateConversation(AddConversationDTO conversationDTO);
    public Task<ConversationDTO> GetConversation(string id);
    public Task UpdateConversation(UpdateConversationDTO conversationDTO);
    public Task DeleteConversation(string id);
}