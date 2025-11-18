using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;
using WTelegram;
using TL;
using ChatModel = TelegramBulkSender.API.Models.Chat;

namespace TelegramBulkSender.API.Services;

public class TelegramService : IAsyncDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly TelegramSessionStorage _sessionStorage;
    private readonly IMemoryCache _memoryCache;
    private readonly SemaphoreSlim _clientLock = new(1, 1);
    private Client? _client;

    public TelegramService(IConfiguration configuration, TelegramSessionStorage sessionStorage, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _sessionStorage = sessionStorage;
        _memoryCache = memoryCache;
        _logger = Log.ForContext<TelegramService>();
    }

    private string? Config(string what)
    {
        return what switch
        {
            "api_id" => _configuration["Telegram:ApiId"] ?? _configuration["TELEGRAM_API_ID"] ?? throw new InvalidOperationException("TELEGRAM_API_ID is not configured"),
            "api_hash" => _configuration["Telegram:ApiHash"] ?? _configuration["TELEGRAM_API_HASH"] ?? throw new InvalidOperationException("TELEGRAM_API_HASH is not configured"),
            "phone_number" => _configuration["Telegram:PhoneNumber"] ?? _configuration["TELEGRAM_PHONE"] ?? throw new InvalidOperationException("TELEGRAM_PHONE is not configured"),
            "session_pathname" => _sessionStorage.SessionPath,
            _ => null
        };
    }

    private async Task<Client> GetClientAsync()
    {
        if (_client != null)
        {
            return _client;
        }

        await _clientLock.WaitAsync();
        try
        {
            if (_client != null)
            {
                return _client;
            }

            _client = new Client(Config);
            try
            {
                var me = await _client.LoginUserIfNeeded();
                _logger.Information("Telegram authenticated as {User}", me.username ?? $"{me.first_name} {me.last_name}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to authenticate Telegram client. Ensure session file exists or complete first authorization.");
                throw;
            }

            return _client;
        }
        finally
        {
            _clientLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<ChatModel>> RefreshChatsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue<IReadOnlyCollection<ChatModel>>("telegram_chats_cache", out var cached))
        {
            return cached;
        }

        var client = await GetClientAsync();
        _logger.Information("Synchronizing chats from Telegram");

        var dialogs = await client.Messages_GetAllDialogs();
        var existing = await dbContext.Chats.ToDictionaryAsync(c => c.TelegramChatId, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var chatBase in dialogs.chats.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tgId = chatBase.ID;
            var title = chatBase.Title ?? string.Empty;
            var type = chatBase.GetType().Name;

            if (!existing.TryGetValue(tgId, out var chat))
            {
                chat = new ChatModel
                {
                    TelegramChatId = tgId,
                    Title = title,
                    Type = type,
                    LastUpdated = now
                };
                dbContext.Chats.Add(chat);
                existing.Add(tgId, chat);
            }
            else
            {
                chat.Title = title;
                chat.Type = type;
                chat.LastUpdated = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var result = (IReadOnlyCollection<ChatModel>)existing.Values.OrderBy(x => x.Title).ToList();
        _memoryCache.Set("telegram_chats_cache", result, TimeSpan.FromMinutes(2));
        return result;
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
            if (!string.IsNullOrWhiteSpace(text))
            {
                dynamic dynamicClient = client;
                await dynamicClient.SendMessageAsync(chatId, text);
            }

            message.IsSuccess = true;
            broadcast.SuccessCount++;
            _logger.Information("Broadcast {BroadcastId}: message sent to chat {ChatId}", broadcast.Id, chatId);
        }
        catch (Exception ex)
        {
            message.IsSuccess = false;
            message.ErrorMessage = ex.Message;
            broadcast.FailedCount++;

            _logger.Warning(ex, "Broadcast {BroadcastId}: failed to send message to chat {ChatId}", broadcast.Id, chatId);

            var floodWaitSeconds = TryGetFloodWaitSeconds(ex);
            if (floodWaitSeconds > 0)
            {
                _logger.Warning("Telegram FloodWait detected, delaying for {Seconds} seconds", floodWaitSeconds);
                await Task.Delay(TimeSpan.FromSeconds(floodWaitSeconds));
            }
        }

        dbContext.BroadcastMessages.Add(message);
        await dbContext.SaveChangesAsync();
        return message;
    }

    private static int TryGetFloodWaitSeconds(Exception ex)
    {
        var message = ex.Message ?? string.Empty;
        var marker = "FLOOD_WAIT_";
        var index = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return 0;
        }

        index += marker.Length;
        var end = index;
        while (end < message.Length && char.IsDigit(message[end]))
        {
            end++;
        }

        return int.TryParse(message[index..end], out var seconds) ? seconds : 0;
    }

    public ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            _client.Dispose();
        }

        return ValueTask.CompletedTask;
    }
}
