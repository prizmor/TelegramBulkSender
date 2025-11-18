using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

public class UserLogsModel : PageModel
{
    private readonly UserLogService _userLogService;

    public UserLogsModel(UserLogService userLogService)
    {
        _userLogService = userLogService;
    }

    public List<UserLog> Logs { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? To { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    public async Task OnGetAsync()
    {
        Logs = await _userLogService.GetLogsAsync(From, To, UserId, Action);
    }

    public async Task<FileResult> OnGetExportAsync()
    {
        var logs = await _userLogService.GetLogsAsync(From, To, UserId, Action);
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("CreatedAt,User,Action,Details");
        foreach (var log in logs)
        {
            var created = log.CreatedAt.ToString("O");
            var user = log.User.Username.Replace("\"", "\"\"");
            var action = log.Action.Replace("\"", "\"\"");
            var details = log.Details.Replace("\"", "\"\"");
            builder.AppendLine($"\"{created}\",\"{user}\",\"{action}\",\"{details}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"user_logs_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
