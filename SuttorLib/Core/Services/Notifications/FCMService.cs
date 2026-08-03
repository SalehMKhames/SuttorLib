using Microsoft.EntityFrameworkCore;
using SuttorLib.Models;
using SuttorLibrary.Data;

namespace SuttorLib.Core.Services.Notifications;

public class FCMService(AppDbContext context) : IFCMService
{
    private readonly AppDbContext _context = context;

    public Task<string?> GetTokenByUserIdAsync(string userId)
    {
        return _context.FCMTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastUsedAt)
            .Select(t => t.Token)
            .FirstOrDefaultAsync();
    }

    public Task<List<string>> GetTokensByUserIdAsync(string userId)
    {
        return _context.FCMTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastUsedAt)
            .Select(t => t.Token)
            .ToListAsync();
    }

    public async Task SaveTokenAsync(FCMToken fcmToken)
    {
        var existing = await _context.FCMTokens
            .FirstOrDefaultAsync(t => t.UserId == fcmToken.UserId && t.Token == fcmToken.Token);

        if (existing is not null)
        {
            existing.LastUsedAt = DateTime.UtcNow;
            existing.DeviceName = fcmToken.DeviceName ?? existing.DeviceName;
            existing.Platform = fcmToken.Platform ?? existing.Platform;
            _context.FCMTokens.Update(existing);
        }
        else
        {
            await _context.FCMTokens.AddAsync(fcmToken);
        }
    }

    public async Task DeleteTokenAsync(string userId, string token)
    {
        var existing = await _context.FCMTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token);

        if (existing is not null)
            _context.FCMTokens.Remove(existing);
    }

    public async Task DeleteAllTokensForUserAsync(string userId)
    {
        var tokens = await _context.FCMTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();

        _context.FCMTokens.RemoveRange(tokens);
    }

    public async Task SaveNotificationToLog(string userId, string title, string body, Dictionary<string, string> type)
    {
        FcmLog log = new FcmLog {
            Id = Guid.NewGuid(),
            Title = title,
            Body = body,
            Type = type
        };

        await _context.FcmLog.AddAsync(log);
        await _context.SaveChangesAsync();

        await LinkUserWithNotification(userId, log.Id);
    }

    public async Task<List<FcmLog>> GetUserNotifications(string userId)
    {
        var logs = await _context.FcmUserLog
            .Where(ul => ul.UserId == userId)
            .Join(_context.FcmLog,
                ul => ul.logId,
                log => log.Id,
                (ul, log) => log)
            .ToListAsync();

        if (logs is null || logs.Count == 0)
            throw new KeyNotFoundException("No Notification");

        return logs;
    }

    private async Task LinkUserWithNotification(string userId, Guid logId)
    {
        var userLog = new FcmUserLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            logId = logId
        };

        await _context.FcmUserLog.AddAsync(userLog);
        await _context.SaveChangesAsync();
    } 
}