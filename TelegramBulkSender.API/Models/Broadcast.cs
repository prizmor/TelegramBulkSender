using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class Broadcast
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [Required]
    [MaxLength(10000)]
    public string TextRu { get; set; } = string.Empty;

    [Required]
    [MaxLength(10000)]
    public string TextEn { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int TotalChats { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public virtual ICollection<BroadcastMessage> Messages { get; set; } = new HashSet<BroadcastMessage>();

    public virtual ICollection<BroadcastFile> Files { get; set; } = new HashSet<BroadcastFile>();
}
