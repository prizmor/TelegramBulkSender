using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class BroadcastService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TelegramService _telegramService;

    public BroadcastService(ApplicationDbContext dbContext, TelegramService telegramService)
    {
        _dbContext = dbContext;
        _telegramService = telegramService;
    }

    public async Task<Broadcast> CreateBroadcastAsync(int userId, string textRu, string textEn, IEnumerable<Chat> chats)
    {
        var chatList = chats.DistinctBy(c => c.Id).ToList();
        var broadcast = new Broadcast
        {
            UserId = userId,
            TextRu = textRu,
            TextEn = textEn,
            CreatedAt = DateTime.UtcNow,
            TotalChats = chatList.Count
        };
        _dbContext.Broadcasts.Add(broadcast);
        await _dbContext.SaveChangesAsync();

        foreach (var chat in chatList)
        {
            var text = BuildTextForChat(chat, textRu, textEn);
            await _telegramService.SendMessageAsync(_dbContext, broadcast, chat.TelegramChatId, text, Enumerable.Empty<string>(), false);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        broadcast.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return broadcast;
    }

    private static string BuildTextForChat(Chat chat, string textRu, string textEn)
    {
        return chat.Language switch
        {
            "ru" => textRu,
            "en" => textEn,
            _ => $"{textRu}\n\n---\n\n{textEn}"
        };
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
