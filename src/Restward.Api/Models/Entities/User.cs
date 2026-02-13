using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [Column("api_key")]
    public string ApiKey { get; set; } = string.Empty;

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Collection> Collections { get; set; } = [];
    public List<Environment> Environments { get; set; } = [];
}
