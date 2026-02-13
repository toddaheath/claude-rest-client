using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

[Table("environment_variables")]
public class EnvironmentVariable
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

    [Column("environment_id")]
    public Guid EnvironmentId { get; set; }

    [ForeignKey(nameof(EnvironmentId))]
    public Entities.Environment Environment { get; set; } = null!;
}
