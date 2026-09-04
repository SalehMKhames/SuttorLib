using SuttorLib.Models;

namespace SuttorLib.Core.Interfaces
{
    public interface IFCM
    {
        Task RegisterTokenAsync(string userId, string token, string? deviceName = null, string? platform = null);
        Task UnregisterTokenAsync(string userId, string token);
        Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
        Task SendToUsersAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data = null);
        Task NotifyNewBlogAsync(string blogPublisherName, string blogTitle, string category);
        Task NotifyNewBookAsync(string bookTitle, List<string> categoryNames, List<string>? authorNames = null);
        Task NotifyBlogCommentAsync(string blogId, string commenterName, string blogOwnerId);
        Task NotifyXPRewardAsync(string userId, int xpGained, int totalXp);
        Task<List<FcmLog>> GetNotificationLogAsync(string userId);
    }
}