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