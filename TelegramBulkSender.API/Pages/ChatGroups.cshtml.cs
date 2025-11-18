using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class ChatGroupsModel : PageModel
{
    private readonly ChatGroupService _chatGroupService;
    private readonly ChatService _chatService;
    private readonly UserLogService _userLogService;

    public ChatGroupsModel(ChatGroupService chatGroupService, ChatService chatService, UserLogService userLogService)
    {
        _chatGroupService = chatGroupService;
        _chatService = chatService;
        _userLogService = userLogService;
    }

    public List<ChatGroup> Groups { get; set; } = new();
    public List<Chat> AllChats { get; set; } = new();

    [BindProperty]
    public GroupInput Input { get; set; } = new();

    public class GroupInput
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public bool IsGlobal { get; set; }
        [Required]
        public List<long> ChatIds { get; set; } = new();
    }

    public async Task OnGetAsync(int? id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        Groups = await _chatGroupService.GetGroupsAsync(userId, includeGlobal: true);
        AllChats = (await _chatService.GetChatsAsync(null)).Where(c => c.IsClient).ToList();

        if (id.HasValue)
        {
            var group = Groups.SingleOrDefault(g => g.Id == id.Value);
            if (group != null)
            {
                Input.Id = group.Id;
                Input.Name = group.Name;
                Input.IsGlobal = group.IsGlobal;
                Input.ChatIds = group.Members.Select(m => m.ChatId).ToList();
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(Input.Id);
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isRoot = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Root");

        if (Input.Id.HasValue)
        {
            await _chatGroupService.UpdateGroupAsync(Input.Id.Value, Input.Name, Input.ChatIds);
            await _userLogService.LogAsync(userId, "UpdateChatGroup", new { Input.Id, Input.Name });
        }
        else
        {
            var group = await _chatGroupService.CreateGroupAsync(Input.Name, Input.IsGlobal, userId, isRoot, Input.ChatIds);
            await _userLogService.LogAsync(userId, "CreateChatGroup", new { group.Id, group.Name, group.IsGlobal });
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isRoot = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Root");
        await _chatGroupService.DeleteGroupAsync(id, userId, isRoot);
        await _userLogService.LogAsync(userId, "DeleteChatGroup", new { id });
        return RedirectToPage();
    }
}
