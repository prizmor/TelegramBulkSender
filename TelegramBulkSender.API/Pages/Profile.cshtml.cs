using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

public class ProfileModel : PageModel
{
    private readonly AuthService _authService;
    private readonly UserLogService _userLogService;

    public ProfileModel(AuthService authService, UserLogService userLogService)
    {
        _authService = authService;
        _userLogService = userLogService;
    }

    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    public class ChangePasswordInput
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            await _authService.ChangeOwnPasswordAsync(userId, Input.CurrentPassword, Input.NewPassword);
            await _userLogService.LogAsync(userId, "ChangeOwnPassword", new { userId });
            return RedirectToPage("/Broadcast");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

