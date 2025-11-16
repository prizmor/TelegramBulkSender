using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

    public async Task OnGetAsync()
    {
        Broadcasts = await _dbContext.Broadcasts.Include(b => b.User).OrderByDescending(b => b.CreatedAt).Take(100).ToListAsync();
    }
}
