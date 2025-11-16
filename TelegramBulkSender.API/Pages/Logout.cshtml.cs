using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

public class LogoutModel : PageModel
{
    private readonly AuthService _authService;

    public LogoutModel(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Request.Cookies.TryGetValue("refresh_token", out var refreshToken);
        await _authService.SignOutAsync(refreshToken);
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return RedirectToPage("/Login");
    }
}
