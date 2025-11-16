using Microsoft.EntityFrameworkCore;
using TelegramBulkSender.API.Models;

namespace TelegramBulkSender.API.Data;

public class ApplicationDbContext : DbContext
{
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
        });

        builder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(x => x.RefreshToken).IsUnique();
        });

        builder.Entity<Chat>(entity =>
        {
            entity.HasIndex(x => x.TelegramChatId).IsUnique();
        });

        builder.Entity<ChatGroup>(entity =>
        {
            entity.HasMany(x => x.Members)
                .WithOne(x => x.ChatGroup)
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MessageTemplate>(entity =>
        {
            entity.HasMany(x => x.Files)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Broadcast>(entity =>
        {
            entity.HasMany(x => x.Messages)
                .WithOne(x => x.Broadcast)
                .HasForeignKey(x => x.BroadcastId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Files)
                .WithOne(x => x.Broadcast)
                .HasForeignKey(x => x.BroadcastId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
