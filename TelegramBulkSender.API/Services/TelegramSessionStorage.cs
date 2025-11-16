namespace TelegramBulkSender.API.Services;

public class TelegramSessionStorage
{
    private readonly string _sessionPath;

    public TelegramSessionStorage(IConfiguration configuration)
    {
        _sessionPath = configuration.GetValue<string>("TELEGRAM_SESSION_PATH") ?? "telegram.session";
    }

    public string SessionPath => _sessionPath;
}
