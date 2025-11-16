using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class UserLog
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string Action { get; set; } = string.Empty;

    [Required]
    public string Details { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
