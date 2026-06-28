using SuttorLib.Models;

namespace SuttorLib.DTOs;

public class AddConversationDTO
{
    public string? Title { get; set; }
    public List<Message> Messages { get; set; } = new();
}

public class ConversationDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAT { get; set; }
    public List<Message> Messages { get; set; } = new();
}

public class UpdateConversationDTO
{
    public string? Title { get; set; }
    public List<Message> Messages { get; set; } = new();
}

public class AddMessageDTO
{
    public Role role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime timestamp { get; set; }
}