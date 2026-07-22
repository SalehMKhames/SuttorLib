namespace SuttorLibrary.DTOs;

public class RegisterFCMTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? Platform { get; set; }
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
    public string blogPublisherName { get; set; }
    public string blogTitle { get; set; }
    public string category { get; set; }
}

public class BookNotificationDTO
{
    public string BookTitle { get; set; }
    public List<string> CategoriesNames { get; set; }
    public string? AuthorName { get; set; }
}

public class CommentNotificationDTO
{
    public string blogId { get; set; } 
    public string blogTitle { get; set; }
    public string commenterName { get; set; }
    public string blogOwnerId { get; set; }
}

public class XpNotificationDTO
{
    public string userId { get; set; }
    public int xpGained { get; set; } 
    public int totalXp { get; set; }
}