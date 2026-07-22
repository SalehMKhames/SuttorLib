namespace SuttorLib.Core.Interfaces
{
    public interface IFCM
    {
        Task RegisterTokenAsync(string userId, string token, string? deviceName = null, string? platform = null);
        Task UnregisterTokenAsync(string userId, string token);
        Task SendToUserAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
        Task NotifyNewBlogAsync(string blogPublisherName, string blogTitle, string category);
        Task NotifyNewBookAsync(string bookTitle, List<string> categoryNames, List<string>? authorName = null);
        Task NotifyBlogCommentAsync(string blogId, string commenterName, string blogOwnerId);
        Task NotifyXPRewardAsync(string userId, int xpGained, int totalXp);
    }
}
