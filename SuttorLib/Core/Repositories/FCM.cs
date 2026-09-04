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
            throw new ArgumentException("FCM token is required", nameof(token));

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
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("FCM token is required", nameof(token));

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

        // Log first, then attempt delivery; stale tokens are cleaned up after the send.
        await _unit.FCMRepo.SaveNotificationToLogAsync(new[] { userId }, title, body, data);

        await SendMulticastAsync(userId, tokens, title, body, data);
    }

    public async Task SendToUsersAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data = null)
    {
        foreach (var userId in userIds.Distinct())
            await SendToUserAsync(userId, title, body, data);
    }

    public async Task NotifyNewBlogAsync(string blogPublisherName, string blogTitle, string category)
    {
        var categories = await _unit.BookRepo.GetCategories();
        var cat = categories?
            .FirstOrDefault(c => c!.Name.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (cat is null)
        {
            _logger.LogWarning("NotifyNewBlogAsync: category {Category} not found, skipping notification", category);
            return;
        }

        var users = await _unit.UserRepo.GetUsersByInterests(cat.Id);
        if (users is null || users.Count == 0)
            return;

        var title = $"{blogPublisherName} published a new blog";
        var body = string.IsNullOrWhiteSpace(category)
            ? blogTitle
            : $"{blogTitle} in {category}";

        await SendToUsersAsync(users.Select(u => u.Id), title, body, new Dictionary<string, string>
        {
            { "type", "new_blog" }
        });
    }

    public async Task NotifyNewBookAsync(string bookTitle, List<string> categoryNames, List<string>? authorNames = null)
    {
        var categories = await _unit.BookRepo.GetCategories();
        var userIds = new HashSet<string>();

        foreach (var name in categoryNames)
        {
            var cat = categories?
                .FirstOrDefault(c => c!.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (cat is null)
            {
                _logger.LogWarning("NotifyNewBookAsync: category {Category} not found, skipping", name);
                continue; // don't abort the whole fan-out for one bad category
            }

            var users = await _unit.UserRepo.GetUsersByInterests(cat.Id);
            if (users is null || users.Count == 0)
                continue; // check remaining categories instead of returning

            foreach (var u in users)
                userIds.Add(u.Id);
        }

        if (userIds.Count == 0)
            return;

        var title = "New book available";
        var body = authorNames is { Count: > 0 }
            ? $"{bookTitle} by {string.Join(", ", authorNames)}"
            : bookTitle;

        if (categoryNames.Count > 0)
            body += $" in {string.Join(", ", categoryNames)}";

        await SendToUsersAsync(userIds, title, body, new Dictionary<string, string>
        {
            { "type", "new_book" }
        });
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

    private async Task SendMulticastAsync(string userId, List<string> tokens, string title, string body, Dictionary<string, string>? data)
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

            // SendEachForMulticastAsync preserves response order matching Tokens order.
            var deadTokens = new List<string>();
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var r = response.Responses[i];
                if (r.IsSuccess)
                    continue;

                _logger.LogWarning(r.Exception, "Failed to send FCM to token {Token}", tokens[i]);

                // Remove tokens FCM reports as permanently invalid so they stop
                // consuming quota and filling the table.
                if (r.Exception is FirebaseMessagingException fex &&
                    fex.MessagingErrorCode is MessagingErrorCode.Unregistered
                        or MessagingErrorCode.InvalidArgument)
                {
                    deadTokens.Add(tokens[i]);
                }
            }

            if (deadTokens.Count > 0)
            {
                await _unit.FCMRepo.DeleteTokensAsync(deadTokens);
                await _unit.CompleteAsync();
                _logger.LogInformation("Removed {Count} stale FCM tokens for user {UserId}", deadTokens.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM multicast");
        }
    }

    public async Task<List<FcmLog>> GetNotificationLogAsync(string userId)
    {
        return await _unit.FCMRepo.GetUserNotificationsAsync(userId);
    }
}