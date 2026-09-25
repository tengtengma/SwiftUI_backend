using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftUI_backend_demo.sources.Models;

[Table("articles")]
public class Article
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? Author { get; set; }

    [Required]
    [Column(TypeName = "text")] // 指定为长文本
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "text")] // 指定为长文本
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")] // 指定为长文本
    public string Url { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? Image { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}