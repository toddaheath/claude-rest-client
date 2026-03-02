using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined
}

[Table("team_invitations")]
public class TeamInvitation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("team_id")]
    public Guid TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("invited_by_id")]
    public Guid InvitedById { get; set; }

    [ForeignKey(nameof(InvitedById))]
    public User InvitedBy { get; set; } = null!;

    [Column("status")]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [Required]
    [MaxLength(256)]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
}
