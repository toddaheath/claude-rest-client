using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

[Table("request_items")]
public class RequestItem
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Column("method")]
    public string Method { get; set; } = "GET";

    [Required]
    [MaxLength(2048)]
    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Column("body")]
    public string? Body { get; set; }

    [MaxLength(100)]
    [Column("body_content_type")]
    public string? BodyContentType { get; set; }

    [Column("collection_id")]
    public Guid CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public Collection Collection { get; set; } = null!;

    [Column("folder_id")]
    public Guid? FolderId { get; set; }

    [ForeignKey(nameof(FolderId))]
    public Folder? Folder { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<RequestHeader> Headers { get; set; } = [];
    public List<RequestParameter> Parameters { get; set; } = [];
}
