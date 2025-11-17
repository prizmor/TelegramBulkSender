using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class TemplatesModel : PageModel
{
    private readonly TemplateService _templateService;

    public TemplatesModel(TemplateService templateService)
    {
        _templateService = templateService;
    }

    public List<MessageTemplate> Templates { get; set; } = new();

    [BindProperty]
    public TemplateInput Input { get; set; } = new();

    public class TemplateInput
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string TextRu { get; set; } = string.Empty;
        [Required]
        public string TextEn { get; set; } = string.Empty;
        public bool IsGlobal { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        Templates = await _templateService.GetTemplatesAsync(userId, includeGlobal: true);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _templateService.CreateTemplateAsync(Input.Name, Input.TextRu, Input.TextEn, Input.IsGlobal, userId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _templateService.DeleteTemplateAsync(id);
        return RedirectToPage();
    }
}
