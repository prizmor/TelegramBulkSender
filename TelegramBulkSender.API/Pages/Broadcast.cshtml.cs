using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class BroadcastModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ChatService _chatService;
    private readonly ChatGroupService _chatGroupService;
    private readonly BroadcastService _broadcastService;

    public BroadcastModel(ApplicationDbContext dbContext, ChatService chatService, ChatGroupService chatGroupService, BroadcastService broadcastService)
    {
        _dbContext = dbContext;
        _chatService = chatService;
        _chatGroupService = chatGroupService;
        _broadcastService = broadcastService;
    }

    public List<Chat> ClientChats { get; set; } = new();
    public List<ChatGroup> AvailableGroups { get; set; } = new();
    public List<Broadcast> RecentBroadcasts { get; set; } = new();

    [BindProperty]
    public BroadcastInput Input { get; set; } = new();

    public class BroadcastInput
    {
        [Required]
        public string TextRu { get; set; } = string.Empty;
        [Required]
        public string TextEn { get; set; } = string.Empty;
        public List<long> SelectedChats { get; set; } = new();
        public List<int> SelectedGroups { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        ClientChats = await _chatService.GetChatsAsync(null);
        AvailableGroups = await _chatGroupService.GetGroupsAsync(userId, includeGlobal: true);
        RecentBroadcasts = await _broadcastService.GetBroadcastsAsync(userId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var chatIds = new HashSet<long>(Input.SelectedChats);
        if (Input.SelectedGroups.Any())
        {
            var groups = await _dbContext.ChatGroups.Include(g => g.Members).Where(g => Input.SelectedGroups.Contains(g.Id)).ToListAsync();
            foreach (var group in groups)
            {
                foreach (var member in group.Members)
                {
                    chatIds.Add(member.ChatId);
                }
            }
        }

        if (!chatIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Выберите хотя бы один чат или группу");
            await OnGetAsync();
            return Page();
        }

        await _broadcastService.CreateBroadcastAsync(userId, Input.TextRu, Input.TextEn, chatIds);
        TempData["StatusMessage"] = $"Рассылка отправлена в {chatIds.Count} чатов";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        ClientChats = await _chatService.GetChatsAsync(null);
        AvailableGroups = await _chatGroupService.GetGroupsAsync(userId, true);
        RecentBroadcasts = await _broadcastService.GetBroadcastsAsync(userId);
        TempData["StatusMessage"] = "Список чатов обновлён";
        return Page();
    }
}
