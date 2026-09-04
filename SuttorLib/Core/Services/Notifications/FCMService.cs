using Microsoft.EntityFrameworkCore;
using SuttorLib.Models;
using SuttorLibrary.Data;
using System.Text.Json;

namespace SuttorLib.Core.Services.Notifications;

public class FCMService(AppDbContext context) : IFCMService
{
    private readonly AppDbContext _context = context;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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

    public async Task DeleteTokensAsync(IEnumerable<string> tokens)
    {
        var tokenList = tokens.ToList();
        if (tokenList.Count == 0)
            return;

        var stale = await _context.FCMTokens
            .Where(t => tokenList.Contains(t.Token))
            .ToListAsync();

        _context.FCMTokens.RemoveRange(stale);
    }

    public async Task DeleteAllTokensForUserAsync(string userId)
    {
        var tokens = await _context.FCMTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();

        _context.FCMTokens.RemoveRange(tokens);
    }

    public async Task<int> PruneStaleTokensAsync(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;

        // Tokens never used since creation, or last used before the cutoff.
        var stale = await _context.FCMTokens
            .Where(t => (t.LastUsedAt ?? t.CreatedAt) < cutoff)
            .ToListAsync();

        _context.FCMTokens.RemoveRange(stale);
        return stale.Count;
    }

    public async Task<Guid> SaveNotificationToLogAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data)
    {
        var log = new FcmLog
        {
            Id = Guid.NewGuid(),
            Title = title,
            Body = body,
            Data = data is null ? null : JsonSerializer.Serialize(data, _jsonOptions),
            CreatedAt = DateTime.UtcNow
        };

        await _context.FcmLog.AddAsync(log);

        foreach (var userId in userIds.Distinct())
        {
            await _context.FcmUserLog.AddAsync(new FcmUserLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LogId = log.Id
            });
        }

        // Single commit for log + user links so the UoW isn't left half-committed.
        await _context.SaveChangesAsync();

        return log.Id;
    }

    public async Task<List<FcmLog>> GetUserNotificationsAsync(string userId)
    {
        return await _context.FcmUserLog
            .AsNoTracking()
            .Where(ul => ul.UserId == userId)
            .Join(_context.FcmLog,
                ul => ul.LogId,
                log => log.Id,
                (ul, log) => log)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();
    }
}