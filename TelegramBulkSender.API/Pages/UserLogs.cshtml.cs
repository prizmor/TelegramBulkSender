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

    public async Task OnGetAsync()
    {
        Logs = await _userLogService.GetLogsAsync();
    }
}
