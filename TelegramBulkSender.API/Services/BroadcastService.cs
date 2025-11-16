using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class BroadcastService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TelegramClientService _telegramClientService;

    public BroadcastService(ApplicationDbContext dbContext, TelegramClientService telegramClientService)
    {
        _dbContext = dbContext;
        _telegramClientService = telegramClientService;
    }

    public async Task<Broadcast> CreateBroadcastAsync(int userId, string textRu, string textEn, IEnumerable<long> chatIds)
    {
        var broadcast = new Broadcast
        {
            UserId = userId,
            TextRu = textRu,
            TextEn = textEn,
            CreatedAt = DateTime.UtcNow,
            TotalChats = chatIds.Count()
        };
        _dbContext.Broadcasts.Add(broadcast);
        await _dbContext.SaveChangesAsync();

        foreach (var chatId in chatIds)
        {
            await _telegramClientService.SendMessageAsync(_dbContext, broadcast, chatId, textRu, Enumerable.Empty<string>(), false);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        broadcast.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return broadcast;
    }

    public async Task<List<Broadcast>> GetBroadcastsAsync(int? userId = null)
    {
        var query = _dbContext.Broadcasts.Include(b => b.User).AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(b => b.UserId == userId.Value);
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }
}
