using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class BroadcastMessage
{
    public int Id { get; set; }

    [Required]
    public int BroadcastId { get; set; }

    [ForeignKey(nameof(BroadcastId))]
    public virtual Broadcast Broadcast { get; set; } = null!;

    [Required]
    public long ChatId { get; set; }

    public int? TelegramMessageId { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime SentAt { get; set; }
}
