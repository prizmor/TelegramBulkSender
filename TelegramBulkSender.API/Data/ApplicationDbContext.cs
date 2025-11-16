using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Data;

public class ApplicationDbContext : DbContext
{
    public const int RootUserId = 1;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserLog> UserLogs => Set<UserLog>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<TemplateFile> TemplateFiles => Set<TemplateFile>();
    public DbSet<Broadcast> Broadcasts => Set<Broadcast>();
    public DbSet<BroadcastMessage> BroadcastMessages => Set<BroadcastMessage>();
    public DbSet<BroadcastFile> BroadcastFiles => Set<BroadcastFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasMany(u => u.Sessions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(new User
            {
                Id = RootUserId,
                Username = "root",
                PasswordHash = string.Empty,
                IsRoot = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(s => s.RefreshToken).IsUnique();
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasIndex(c => c.TelegramChatId).IsUnique();
        });

        modelBuilder.Entity<ChatGroup>(entity =>
        {
            entity.HasMany(g => g.Members)
                .WithOne(m => m.ChatGroup)
                .HasForeignKey(m => m.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatGroupMember>(entity =>
        {
            entity.HasIndex(m => new { m.ChatGroupId, m.ChatId }).IsUnique();
        });

        modelBuilder.Entity<MessageTemplate>(entity =>
        {
            entity.HasMany(t => t.Files)
                .WithOne(f => f.Template)
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Broadcast>(entity =>
        {
            entity.HasMany(b => b.Messages)
                .WithOne(m => m.Broadcast)
                .HasForeignKey(m => m.BroadcastId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(b => b.Files)
                .WithOne(f => f.Broadcast)
                .HasForeignKey(f => f.BroadcastId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
