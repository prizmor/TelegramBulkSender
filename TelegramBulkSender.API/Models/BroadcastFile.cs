using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class BroadcastFile
{
    public int Id { get; set; }

    [Required]
    public int BroadcastId { get; set; }

    [ForeignKey(nameof(BroadcastId))]
    public virtual Broadcast Broadcast { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    public bool IsImage { get; set; }

    public long FileSize { get; set; }
}
