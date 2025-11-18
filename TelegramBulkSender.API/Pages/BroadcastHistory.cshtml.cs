using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Pages;

[Authorize]
public class BroadcastHistoryModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public BroadcastHistoryModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Broadcast> Broadcasts { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? To { get; set; }

    public async Task OnGetAsync()
    {
        var query = _dbContext.Broadcasts.Include(b => b.User).AsQueryable();

        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isRoot = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Root");

        if (!isRoot)
        {
            query = query.Where(b => b.UserId == currentUserId);
        }
        else if (UserId.HasValue)
        {
            query = query.Where(b => b.UserId == UserId.Value);
        }

        if (From.HasValue)
        {
            query = query.Where(b => b.CreatedAt >= From.Value);
        }

        if (To.HasValue)
        {
            query = query.Where(b => b.CreatedAt <= To.Value);
        }

        Broadcasts = await query.OrderByDescending(b => b.CreatedAt).Take(200).ToListAsync();
    }
}
