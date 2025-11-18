using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class TemplateService
{
    private readonly ApplicationDbContext _dbContext;

    public TemplateService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MessageTemplate>> GetTemplatesAsync(int userId, bool includeGlobal)
    {
        var query = _dbContext.MessageTemplates.Include(t => t.Files).AsQueryable();
        if (includeGlobal)
        {
            query = query.Where(t => t.IsGlobal || t.UserId == userId);
        }
        else
        {
            query = query.Where(t => !t.IsGlobal && t.UserId == userId);
        }

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<MessageTemplate> CreateTemplateAsync(string name, string textRu, string textEn, bool isGlobal, int userId, bool isUserRoot)
    {
        if (isGlobal && !isUserRoot)
        {
            throw new InvalidOperationException("Only root user can create global templates");
        }

        var template = new MessageTemplate
        {
            Name = name,
            TextRu = textRu,
            TextEn = textEn,
            IsGlobal = isGlobal,
            UserId = isGlobal ? null : userId,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.MessageTemplates.Add(template);
        await _dbContext.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(int id, string name, string textRu, string textEn)
    {
        var template = await _dbContext.MessageTemplates.FindAsync(id) ?? throw new KeyNotFoundException("Template not found");
        template.Name = name;
        template.TextRu = textRu;
        template.TextEn = textEn;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _dbContext.MessageTemplates.FindAsync(id) ?? throw new KeyNotFoundException("Template not found");
        _dbContext.MessageTemplates.Remove(template);
        await _dbContext.SaveChangesAsync();
    }
}
