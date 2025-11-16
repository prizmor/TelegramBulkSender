using System.ComponentModel.DataAnnotations;

namespace TelegramBulkSender.API.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsRoot { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<UserSession> Sessions { get; set; } = new HashSet<UserSession>();

    public virtual ICollection<UserLog> Logs { get; set; } = new HashSet<UserLog>();

    public virtual ICollection<ChatGroup> ChatGroups { get; set; } = new HashSet<ChatGroup>();

    public virtual ICollection<MessageTemplate> Templates { get; set; } = new HashSet<MessageTemplate>();

    public virtual ICollection<Broadcast> Broadcasts { get; set; } = new HashSet<Broadcast>();
}
