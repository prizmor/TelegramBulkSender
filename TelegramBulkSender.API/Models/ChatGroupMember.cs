using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class ChatGroupMember
{
    public int Id { get; set; }

    [Required]
    public int ChatGroupId { get; set; }

    [ForeignKey(nameof(ChatGroupId))]
    public virtual ChatGroup ChatGroup { get; set; } = null!;

    [Required]
    public long ChatId { get; set; }

    [ForeignKey(nameof(ChatId))]
    public virtual Chat? Chat { get; set; }
}
