using Microsoft.EntityFrameworkCore;
using SuttorLib.Models;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.Models;

namespace SuttorLib.Core.Services.Notifications;

public class FCMRepository(AppDbContext context) : IFCMRepository
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
}