using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class ChatGroupsModel : PageModel
{
    private readonly ChatGroupService _chatGroupService;
    private readonly ChatService _chatService;

    public ChatGroupsModel(ChatGroupService chatGroupService, ChatService chatService)
    {
        _chatGroupService = chatGroupService;
        _chatService = chatService;
    }

    public List<ChatGroup> Groups { get; set; } = new();
    public List<Chat> AllChats { get; set; } = new();

    [BindProperty]
    public GroupInput Input { get; set; } = new();

    public class GroupInput
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public bool IsGlobal { get; set; }
        [Required]
        public List<long> ChatIds { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        Groups = await _chatGroupService.GetGroupsAsync(userId, includeGlobal: true);
        AllChats = await _chatService.GetChatsAsync(null);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _chatGroupService.CreateGroupAsync(Input.Name, Input.IsGlobal, userId, Input.ChatIds);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _chatGroupService.DeleteGroupAsync(id);
        return RedirectToPage();
    }
}
