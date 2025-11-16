using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBulkSender.API.Models;

public class TemplateFile
{
    public int Id { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public virtual MessageTemplate Template { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    public bool IsImage { get; set; }

    public long FileSize { get; set; }
}
