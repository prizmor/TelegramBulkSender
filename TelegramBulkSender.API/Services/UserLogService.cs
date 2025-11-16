using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class UserLogService
{
    private readonly ApplicationDbContext _dbContext;

    public UserLogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(int userId, string action, object? details = null)
    {
        var log = new UserLog
        {
            UserId = userId,
            Action = action,
            Details = details == null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(details),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.UserLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<UserLog>> GetLogsAsync(DateTime? from = null, DateTime? to = null, int? userId = null, string? action = null)
    {
        var query = _dbContext.UserLogs.Include(l => l.User).AsQueryable();
        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= to.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(l => l.Action == action);
        }

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }
}
