using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class TelegramService : IAsyncDisposable
{
    private readonly Serilog.ILogger _logger;

    public TelegramService(IConfiguration configuration, TelegramSessionStorage sessionStorage, IMemoryCache memoryCache)
    {
        _logger = Log.ForContext<TelegramService>();
    }

    public async Task<IReadOnlyCollection<Chat>> RefreshChatsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        _logger.Information("RefreshChatsAsync called; returning chats from database without Telegram sync");
        return await dbContext.Chats.AsNoTracking().OrderBy(x => x.Title).ToListAsync(cancellationToken);
    }

    public async Task<BroadcastMessage> SendMessageAsync(ApplicationDbContext dbContext, Broadcast broadcast, long chatId, string text, IEnumerable<string> filePaths, bool isImage)
    {
        var message = new BroadcastMessage
        {
            BroadcastId = broadcast.Id,
            ChatId = chatId,
            SentAt = DateTime.UtcNow,
            IsSuccess = false,
            ErrorMessage = "Telegram sending is disabled in this build"
        };

        broadcast.FailedCount++;
        _logger.Warning("Attempted to send Telegram message for chat {ChatId}, but Telegram sending is disabled", chatId);

        dbContext.BroadcastMessages.Add(message);
        await dbContext.SaveChangesAsync();
        return message;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
