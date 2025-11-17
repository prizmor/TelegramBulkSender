using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

public class UsersModel : PageModel
{
    private readonly AuthService _authService;

    public UsersModel(AuthService authService)
    {
        _authService = authService;
    }

    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    public class CreateUserInput
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool IsRoot { get; set; }
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

        await _authService.CreateUserAsync(Input.Username, Input.Password, Input.IsRoot);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _authService.DeleteUserAsync(id);
        return RedirectToPage();
    }
}
