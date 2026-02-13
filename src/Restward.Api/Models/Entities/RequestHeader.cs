using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

[Table("request_headers")]
public class RequestHeader
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [MaxLength(4000)]
    [Column("value")]
    public string Value { get; set; } = string.Empty;

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("request_item_id")]
    public Guid RequestItemId { get; set; }

    [ForeignKey(nameof(RequestItemId))]
    public RequestItem RequestItem { get; set; } = null!;
}
