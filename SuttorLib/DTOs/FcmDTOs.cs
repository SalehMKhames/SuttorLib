namespace SuttorLibrary.DTOs;

public class RegisterFCMTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? Platform { get; set; }
}

public class UnregisterFCMTokenDto
{
    public string Token { get; set; } = string.Empty;
}

public class SendNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string>? Data { get; set; }
}

public class BlogNotificationDTO
{
    public string BlogPublisherName { get; set; } = string.Empty;
    public string BlogTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class BookNotificationDTO
{
    public string BookTitle { get; set; } = string.Empty;
    public List<string> CategoriesNames { get; set; } = new();
    public string? AuthorName { get; set; }
}

public class CommentNotificationDTO
{
    public string BlogId { get; set; } = string.Empty;
    public string BlogTitle { get; set; } = string.Empty;
    public string CommenterName { get; set; } = string.Empty;
    public string BlogOwnerId { get; set; } = string.Empty;
}

public class XpNotificationDTO
{
    public string UserId { get; set; } = string.Empty;
    public int XpGained { get; set; }
    public int TotalXp { get; set; }
}