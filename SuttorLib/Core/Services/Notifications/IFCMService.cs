using SuttorLib.Models;

namespace SuttorLib.Core.Services.Notifications
{
    public interface IFCMService
    {
        Task<List<string>> GetTokensByUserIdAsync(string userId);
        Task SaveTokenAsync(FCMToken fcmToken);
        Task DeleteTokenAsync(string userId, string token);
        Task DeleteTokensAsync(IEnumerable<string> tokens);
        Task DeleteAllTokensForUserAsync(string userId);
        Task<int> PruneStaleTokensAsync(TimeSpan maxAge);

        /// <summary>
        /// Persists one log entry and links it to every given user in a single SaveChanges call.
        /// Returns the new log id.
        /// </summary>
        Task<Guid> SaveNotificationToLogAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data);

        Task<List<FcmLog>> GetUserNotificationsAsync(string userId);
    }
}