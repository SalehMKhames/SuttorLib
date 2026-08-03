using FirebaseAdmin.Messaging;
using SuttorLib.Core.Interfaces;
using SuttorLib.Models;
using SuttorLib.Models.Library;
using SuttorLibrary.Core;
using SuttorLibrary.Models;

namespace SuttorLib.Core.Repositories;

public class FCM(IUnitOfWork unit, ILogger<FCM> logger) : IFCM
{
    private readonly IUnitOfWork _unit = unit;
    private readonly ILogger<FCM> _logger = logger;

    public async Task RegisterTokenAsync(string userId, string token, string? deviceName = null, string? platform = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("FCM token is required");

        await _unit.FCMRepo.SaveTokenAsync(new FCMToken
        {
            UserId = userId,
            Token = token,
            DeviceName = deviceName,
            Platform = platform
        });

        await _unit.CompleteAsync();
    }

    public async Task UnregisterTokenAsync(string userId, string token)
    {
        await _unit.FCMRepo.DeleteTokenAsync(userId, token);
        await _unit.CompleteAsync();
    }

    public async Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokens = await _unit.FCMRepo.GetTokensByUserIdAsync(userId);
        if (tokens.Count == 0)
        {
            _logger.LogInformation("No FCM tokens found for user {UserId}", userId);
            return;
        }

        await _unit.FCMRepo.SaveNotificationToLog(userId, title, body, data);

        await SendMulticastAsync(tokens, title, body, data);
    }

    public async Task NotifyNewBlogAsync(string blogPublisherName, string blogTitle, string category)
    {
        var categories = await _unit.BookRepo.GetCategories();
        var cat = categories!
            .Where(c => c!.Name.Equals(category, StringComparison.CurrentCultureIgnoreCase))
            .ToList()
            .FirstOrDefault();

        var users = await _unit.UserRepo.GetUsersByInterests(cat!.Id);
        if (users is null || users.Count == 0)
            return;
        
        var title = $"{blogPublisherName} published a new blog";
        var body = string.IsNullOrWhiteSpace(category)
            ? blogTitle
            : $"{blogTitle} in {category}";

        foreach (var user in users)
        {
            await SendToUserAsync(user.Id, title, body, new Dictionary<string, string>
            {
                { "type", "new_blog" }
            });
        }
    }

    public async Task NotifyNewBookAsync(string bookTitle, List<string> categoryNames, List<string>? authorNames = null)
    {
        var categories = await _unit.BookRepo.GetCategories();
        var cat = new List<Category>();
        var users = new List<AppUser>();

        foreach (var name in categoryNames)
            cat.Add(categories!
                .Where(c => c!.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                .ToList()
                .FirstOrDefault()!
            );

        foreach (var c in cat)
        {
            var user = await _unit.UserRepo.GetUsersByInterests(c!.Id);
            if (user is null || user.Count == 0)
                return;
            users.AddRange(user);
        }

        var title = "New book available";
        var body = authorNames is null || authorNames.Any()
            ? $"{bookTitle}"
            : $"{bookTitle} by {authorNames}";

        if (categoryNames.Any())
            body += $" in {string.Join(", ", categoryNames)}";

        foreach (var user in users)
        {
            await SendToUserAsync(user.Id, title, body, new Dictionary<string, string>
            {
                { "type", "new_book" }
            });
        }
    }

    public async Task NotifyBlogCommentAsync(string blogId, string commenterName, string blogOwnerId)
    {
        await SendToUserAsync(blogOwnerId, "New reply on your blog", $"{commenterName} replied to your blog", new Dictionary<string, string>
        {
            { "type", "blog_reply" },
            { "blogId", blogId }
        });
    }

    public async Task NotifyXPRewardAsync(string userId, int xpGained, int totalXp)
    {
        await SendToUserAsync(userId, "XP Reward!", $"You earned {xpGained} XP. Total: {totalXp} XP", new Dictionary<string, string>
        {
            { "type", "xp_reward" },
            { "xpGained", xpGained.ToString() },
            { "totalXp", totalXp.ToString() }
        });
    }

    private async Task SendMulticastAsync(List<string> tokens, string title, string body, Dictionary<string, string>? data)
    {
        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data,
            Android = new AndroidConfig
            {
                Priority = Priority.High
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = "default",
                    ContentAvailable = true
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("FCM multicast sent: {SuccessCount} successful, {FailureCount} failed",
                response.SuccessCount, response.FailureCount);

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    _logger.LogWarning(response.Responses[i].Exception, "Failed to send FCM to token {Token}", tokens[i]);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM multicast");
        }
    }

    public async Task<List<FcmLog>?> NotificationLog(string userId)
    {
        var log = await _unit.FCMRepo.GetUserNotifications(userId);
        if (log is null || log.Count == 0)
            return null;

        return log;
    }
}