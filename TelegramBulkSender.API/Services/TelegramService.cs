using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;
using TL;
using WTelegram;

namespace TelegramBulkSender.API.Services;

public class TelegramService : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly TelegramSessionStorage _sessionStorage;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private Client? _client;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TelegramService(IConfiguration configuration, TelegramSessionStorage sessionStorage, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _sessionStorage = sessionStorage;
        _memoryCache = memoryCache;
        _logger = Log.ForContext<TelegramService>();
    }

    public async Task<Client> GetClientAsync()
    {
        if (_client != null)
        {
            return _client;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_client != null)
            {
                return _client;
            }

            var apiId = _configuration.GetValue<int>("TELEGRAM_API_ID");
            var apiHash = _configuration.GetValue<string>("TELEGRAM_API_HASH") ?? throw new InvalidOperationException("Missing TELEGRAM_API_HASH");
            var phone = _configuration.GetValue<string>("TELEGRAM_PHONE") ?? throw new InvalidOperationException("Missing TELEGRAM_PHONE");

            _client = new Client(config => config switch
            {
                "api_id" => apiId,
                "api_hash" => apiHash,
                "phone_number" => phone,
                "session_pathname" => _sessionStorage.SessionPath,
                _ => null
            });

            _logger.Information("Telegram client initialized");
            return _client;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyCollection<Chat>> RefreshChatsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var client = await GetClientAsync();
        var chats = await client.Messages_GetAllChats();
        var now = DateTime.UtcNow;
        foreach (var (_, chatBase) in chats.chats)
        {
            if (chatBase is not TL.ChatBase chat)
            {
                continue;
            }

            var title = chat.Title ?? chat.ToString();
            var entity = await dbContext.Chats.SingleOrDefaultAsync(c => c.TelegramChatId == chat.ID, cancellationToken);
            if (entity == null)
            {
                entity = new Chat
                {
                    TelegramChatId = chat.ID,
                    Title = title,
                    Type = chat.GetType().Name,
                    LastUpdated = now
                };
                dbContext.Chats.Add(entity);
            }
            else
            {
                entity.Title = title;
                entity.Type = chat.GetType().Name;
                entity.LastUpdated = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await dbContext.Chats.AsNoTracking().OrderBy(x => x.Title).ToListAsync(cancellationToken);
    }

    public async Task<BroadcastMessage> SendMessageAsync(ApplicationDbContext dbContext, Broadcast broadcast, long chatId, string text, IEnumerable<string> filePaths, bool isImage)
    {
        var client = await GetClientAsync();
        var message = new BroadcastMessage
        {
            BroadcastId = broadcast.Id,
            ChatId = chatId,
            SentAt = DateTime.UtcNow
        };

        try
        {
            foreach (var file in filePaths)
            {
                await client.SendDocumentAsync(chatId, file, caption: text, force_document: !isImage);
            }

            if (!filePaths.Any())
            {
                await client.SendMessageAsync(chatId, text);
            }

            message.IsSuccess = true;
            broadcast.SuccessCount++;
        }
        catch (FloodWaitException ex)
        {
            _logger.Warning("Flood wait detected for {ChatId}: {Seconds}", chatId, ex.TimeToWait);
            await Task.Delay(ex.TimeToWait);
            message.IsSuccess = false;
            message.ErrorMessage = ex.Message;
            broadcast.FailedCount++;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send message to chat {ChatId}", chatId);
            message.IsSuccess = false;
            message.ErrorMessage = ex.Message;
            broadcast.FailedCount++;
        }

        dbContext.BroadcastMessages.Add(message);
        await dbContext.SaveChangesAsync();
        return message;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
