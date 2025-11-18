using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TelegramBulkSender.API.Models;
using TelegramBulkSender.API.Services;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class TemplatesModel : PageModel
{
    private readonly TemplateService _templateService;
    private readonly UserLogService _userLogService;

    public TemplatesModel(TemplateService templateService, UserLogService userLogService)
    {
        _templateService = templateService;
        _userLogService = userLogService;
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
        var isRoot = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Root");
        var template = await _templateService.CreateTemplateAsync(Input.Name, Input.TextRu, Input.TextEn, Input.IsGlobal, userId, isRoot);
        await _userLogService.LogAsync(userId, "CreateTemplate", new { template.Id, template.Name, template.IsGlobal });
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string textRu, string textEn)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(textRu) || string.IsNullOrWhiteSpace(textEn))
        {
            ModelState.AddModelError(string.Empty, "Все поля шаблона обязательны");
            await OnGetAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _templateService.UpdateTemplateAsync(id, name, textRu, textEn);
        await _userLogService.LogAsync(userId, "UpdateTemplate", new { id, name });
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _templateService.DeleteTemplateAsync(id);
        await _userLogService.LogAsync(userId, "DeleteTemplate", new { id });
        return RedirectToPage();
    }
}
