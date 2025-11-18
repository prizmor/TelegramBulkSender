using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

public class UsersModel : PageModel
{
    private readonly AuthService _authService;
    private readonly UserLogService _userLogService;

    public UsersModel(AuthService authService, UserLogService userLogService)
    {
        _authService = authService;
        _userLogService = userLogService;
    }

    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    [BindProperty]
    public ChangePasswordInput PasswordInput { get; set; } = new();

    public class CreateUserInput
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool IsRoot { get; set; }
    }

    public class ChangePasswordInput
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        Users = await _authService.GetUsersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var created = await _authService.CreateUserAsync(Input.Username, Input.Password, Input.IsRoot);
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(currentUserId, "CreateUser", new { created.Id, created.Username, created.IsRoot });
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _authService.DeleteUserAsync(id);
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(currentUserId, "DeleteUser", new { UserId = id });
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        await _authService.UpdatePasswordAsync(PasswordInput.UserId, PasswordInput.NewPassword);
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _userLogService.LogAsync(currentUserId, "ChangeUserPassword", new { PasswordInput.UserId });
        return RedirectToPage();
    }
}
