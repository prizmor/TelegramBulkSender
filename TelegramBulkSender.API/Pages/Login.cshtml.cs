using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Services;
using TelegramBulkSender.API.ViewModels;

namespace TelegramBulkSender.API.Pages;

public class LoginModel : PageModel
{
    private readonly AuthService _authService;

    public LoginModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.AuthenticateAsync(Input.Username, Input.Password);
        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
            return Page();
        }

        Response.Cookies.Append("access_token", result.Value.accessToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = result.Value.expires });
        Response.Cookies.Append("refresh_token", result.Value.refreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddDays(7) });
        return RedirectToPage("/Broadcast");
    }
}
