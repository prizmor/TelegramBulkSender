using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class ChatsModel : PageModel
{
    private readonly ChatService _chatService;
    private readonly TelegramService _telegramService;
    private readonly ApplicationDbContext _dbContext;

    public ChatsModel(ChatService chatService, TelegramService telegramService, ApplicationDbContext dbContext)
    {
        _chatService = chatService;
        _telegramService = telegramService;
        _dbContext = dbContext;
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
        return RedirectToPage(new { search = Search });
    }

    public async Task<IActionResult> OnPostToggleSystemAsync(long chatId, bool value)
    {
        await _chatService.UpdateChatFlagsAsync(chatId, isSystem: value);
        return RedirectToPage(new { search = Search });
    }

    public async Task<IActionResult> OnPostLanguageAsync(long chatId, string? language)
    {
        await _chatService.UpdateChatFlagsAsync(chatId, language: language);
        return RedirectToPage(new { search = Search });
    }
}
