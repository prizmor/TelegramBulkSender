using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize(Policy = "RootOnly")]
public class ChatsModel : PageModel
{
    private readonly ChatService _chatService;
    private readonly TelegramService _telegramService;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserLogService _userLogService;

    public ChatsModel(ChatService chatService, TelegramService telegramService, ApplicationDbContext dbContext, UserLogService userLogService)
    {
        _chatService = chatService;
        _telegramService = telegramService;
        _dbContext = dbContext;
        _userLogService = userLogService;
    }

    public List<Models.Chat> Chats { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Chats = await _chatService.GetChatsAsync(Search);
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        await _telegramService.RefreshChatsAsync(_dbContext, HttpContext.RequestAborted);
        return RedirectToPage(new { search = Search });
    }

    public async Task<IActionResult> OnPostToggleClientAsync(long chatId, bool value)
    {
        await _chatService.UpdateChatFlagsAsync(chatId, isClient: value);
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(userId, "UpdateChatClientFlag", new { chatId, value });
        return RedirectToPage(new { search = Search });
    }

    public async Task<IActionResult> OnPostToggleSystemAsync(long chatId, bool value)
    {
        await _chatService.UpdateChatFlagsAsync(chatId, isSystem: value);
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(userId, "UpdateChatSystemFlag", new { chatId, value });
        return RedirectToPage(new { search = Search });
    }

    public async Task<IActionResult> OnPostLanguageAsync(long chatId, string? language)
    {
        await _chatService.UpdateChatFlagsAsync(chatId, language: language);
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(userId, "UpdateChatLanguage", new { chatId, language });
        return RedirectToPage(new { search = Search });
    }
}
