using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Data;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Services;

public class ChatGroupService
{
    private readonly ApplicationDbContext _dbContext;

    public ChatGroupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ChatGroup>> GetGroupsAsync(int userId, bool includeGlobal)
    {
        var query = _dbContext.ChatGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.Chat)
            .AsQueryable();
        if (!includeGlobal)
        {
            query = query.Where(g => !g.IsGlobal && g.UserId == userId);
        }
        else
        {
            query = query.Where(g => g.IsGlobal || g.UserId == userId);
        }

        return await query.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<ChatGroup> CreateGroupAsync(string name, bool isGlobal, int? userId, IEnumerable<long> chatIds)
    {
        var chatIdList = chatIds.Distinct().ToList();
        var existingChatIds = await _dbContext.Chats
            .Where(c => chatIdList.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var missingIds = chatIdList.Except(existingChatIds).ToList();
        if (missingIds.Count > 0)
        {
            throw new InvalidOperationException($"Chats not found: {string.Join(",", missingIds)}");
        }

        var group = new ChatGroup
        {
            Name = name,
            IsGlobal = isGlobal,
            UserId = isGlobal ? null : userId,
            CreatedAt = DateTime.UtcNow,
            Members = chatIdList.Select(id => new ChatGroupMember { ChatId = id }).ToList()
        };
        _dbContext.ChatGroups.Add(group);
        await _dbContext.SaveChangesAsync();
        return group;
    }

    public async Task UpdateGroupAsync(int groupId, string name, IEnumerable<long> chatIds)
    {
        var chatIdList = chatIds.Distinct().ToList();
        var existingChatIds = await _dbContext.Chats
            .Where(c => chatIdList.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var missingIds = chatIdList.Except(existingChatIds).ToList();
        if (missingIds.Count > 0)
        {
            throw new InvalidOperationException($"Chats not found: {string.Join(",", missingIds)}");
        }

        var group = await _dbContext.ChatGroups.Include(g => g.Members).SingleOrDefaultAsync(g => g.Id == groupId) ?? throw new KeyNotFoundException("Group not found");
        group.Name = name;
        _dbContext.ChatGroupMembers.RemoveRange(group.Members);
        group.Members = chatIdList.Select(id => new ChatGroupMember { ChatId = id }).ToList();
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        var group = await _dbContext.ChatGroups.FindAsync(groupId) ?? throw new KeyNotFoundException("Group not found");
        _dbContext.ChatGroups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }
}
