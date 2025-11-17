using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class MessageTemplate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2048)]
    public string TextRu { get; set; } = string.Empty;

    [Required]
    [MaxLength(2048)]
    public string TextEn { get; set; } = string.Empty;

    public bool IsGlobal { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TemplateFile> Files { get; set; } = new HashSet<TemplateFile>();
}
