using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBulkSender.API.Data;

namespace TelegramBulkSender.API.Services;

public class TelegramSyncHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramSyncHostedService> _logger;

    public TelegramSyncHostedService(IServiceProvider serviceProvider, ILogger<TelegramSyncHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var telegramClient = scope.ServiceProvider.GetRequiredService<TelegramClientService>();
                await telegramClient.RefreshChatsAsync(dbContext, stoppingToken);
                _logger.LogInformation("Telegram chats synchronized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize Telegram chats");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
