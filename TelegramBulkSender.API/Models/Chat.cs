using System.ComponentModel.DataAnnotations;

namespace TelegramBulkSender.API.Models;

public class Chat
{
    public long Id { get; set; }

    [Required]
    public long TelegramChatId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Type { get; set; } = string.Empty;

    public bool IsClient { get; set; }

    public bool IsSystemChat { get; set; }

    [MaxLength(8)]
    public string? Language { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual ICollection<ChatGroupMember> Groups { get; set; } = new HashSet<ChatGroupMember>();
}
