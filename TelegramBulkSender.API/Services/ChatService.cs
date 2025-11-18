using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class ChatService
{
    private readonly ApplicationDbContext _dbContext;

    public ChatService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Chat>> GetChatsAsync(string? search)
    {
        var query = _dbContext.Chats.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.Contains(search));
        }

        return await query.OrderBy(c => c.Title).ToListAsync();
    }

    public async Task UpdateChatFlagsAsync(long chatId, bool? isClient = null, bool? isSystem = null, string? language = null)
    {
        var chat = await _dbContext.Chats.SingleOrDefaultAsync(c => c.Id == chatId) ?? throw new KeyNotFoundException("Chat not found");
        if (isClient.HasValue)
        {
            chat.IsClient = isClient.Value;
        }

        if (isSystem.HasValue)
        {
            chat.IsSystemChat = isSystem.Value;
        }

        if (language != null)
        {
            chat.Language = string.IsNullOrEmpty(language) ? null : language;
        }

        await _dbContext.SaveChangesAsync();
    }
}
