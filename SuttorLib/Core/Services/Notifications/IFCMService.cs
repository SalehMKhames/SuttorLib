using SuttorLib.Models;

namespace SuttorLib.Core.Services.Notifications
{
    public interface IFCMService
    {
        Task<string?> GetTokenByUserIdAsync(string userId);
        Task<List<string>> GetTokensByUserIdAsync(string userId);
        Task SaveTokenAsync(FCMToken fcmToken);
        Task DeleteTokenAsync(string userId, string token);
        Task DeleteAllTokensForUserAsync(string userId);
    }
}
