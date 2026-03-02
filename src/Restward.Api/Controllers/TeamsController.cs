using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/teams")]
[EnableRateLimiting("standard")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeamsController(AppDbContext db)
    {
        _db = db;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    /// <summary>Lists all teams the current user is a member of.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TeamDto>>> GetAll()
    {
        var userId = GetCurrentUser().Id;

        var teams = await _db.TeamMembers
            .Where(tm => tm.UserId == userId)
            .Include(tm => tm.Team)
            .Select(tm => new TeamDto
            {
                Id = tm.Team.Id,
                Name = tm.Team.Name,
                OwnerId = tm.Team.OwnerId,
                CreatedAt = tm.Team.CreatedAt
            })
            .ToListAsync();

        return Ok(teams);
    }

    /// <summary>Creates a new team. The creator becomes the Owner.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamDto>> Create([FromBody] CreateTeamDto dto)
    {
        var user = GetCurrentUser();

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = user.Id,
            Role = TeamRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _db.Teams.Add(team);
        _db.TeamMembers.Add(member);
        await _db.SaveChangesAsync();

        return Created($"/api/teams/{team.Id}", new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            OwnerId = team.OwnerId,
            CreatedAt = team.CreatedAt
        });
    }

    /// <summary>Gets a team by ID, including its members. Only accessible to team members.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamDto>> GetById(Guid id)
    {
        var userId = GetCurrentUser().Id;

        var isMember = await _db.TeamMembers
            .AnyAsync(tm => tm.TeamId == id && tm.UserId == userId);

        if (!isMember)
            return NotFound();

        var team = await _db.Teams
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team is null)
            return NotFound();

        return Ok(new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            OwnerId = team.OwnerId,
            CreatedAt = team.CreatedAt,
            Members = team.Members.Select(m => new TeamMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = m.User.Name,
                UserEmail = m.User.Email,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList()
        });
    }

    /// <summary>Deletes a team. Only the team owner can delete.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUser().Id;

        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (team is null)
            return NotFound();

        if (team.OwnerId != userId)
            return StatusCode(StatusCodes.Status403Forbidden);

        _db.Teams.Remove(team);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Invites a user by email to a team. Only Owner/Admin can invite.</summary>
    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType(typeof(TeamInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TeamInvitationDto>> Invite(Guid id, [FromBody] InviteDto dto)
    {
        var user = GetCurrentUser();

        var membership = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == id && tm.UserId == user.Id);

        if (membership is null)
            return NotFound();

        if (membership.Role != TeamRole.Owner && membership.Role != TeamRole.Admin)
            return StatusCode(StatusCodes.Status403Forbidden);

        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (team is null)
            return NotFound();

        var invitation = new TeamInvitation
        {
            Id = Guid.NewGuid(),
            TeamId = id,
            Email = dto.Email,
            InvitedById = user.Id,
            Status = InvitationStatus.Pending,
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.TeamInvitations.Add(invitation);
        await _db.SaveChangesAsync();

        return Created($"/api/teams/invitations/{invitation.Id}", new TeamInvitationDto
        {
            Id = invitation.Id,
            TeamId = invitation.TeamId,
            TeamName = team.Name,
            Email = invitation.Email,
            InvitedByName = user.Name,
            Status = invitation.Status.ToString(),
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt
        });
    }

    /// <summary>Lists pending invitations for the current user (matched by email).</summary>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(List<TeamInvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TeamInvitationDto>>> GetInvitations()
    {
        var user = GetCurrentUser();

        var invitations = await _db.TeamInvitations
            .Include(i => i.Team)
            .Include(i => i.InvitedBy)
            .Where(i => i.Email == user.Email
                && i.Status == InvitationStatus.Pending
                && i.ExpiresAt > DateTime.UtcNow)
            .Select(i => new TeamInvitationDto
            {
                Id = i.Id,
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                Email = i.Email,
                InvitedByName = i.InvitedBy.Name,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                ExpiresAt = i.ExpiresAt
            })
            .ToListAsync();

        return Ok(invitations);
    }

    /// <summary>Accepts a pending invitation, creating a TeamMember record.</summary>
    [HttpPost("invitations/{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptInvitation(Guid id)
    {
        var user = GetCurrentUser();

        var invitation = await _db.TeamInvitations
            .FirstOrDefaultAsync(i => i.Id == id
                && i.Email == user.Email
                && i.Status == InvitationStatus.Pending
                && i.ExpiresAt > DateTime.UtcNow);

        if (invitation is null)
            return NotFound();

        // Check if already a member
        var alreadyMember = await _db.TeamMembers
            .AnyAsync(tm => tm.TeamId == invitation.TeamId && tm.UserId == user.Id);

        if (alreadyMember)
            return Conflict(new { error = "User is already a member of this team" });

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = invitation.TeamId,
            UserId = user.Id,
            Role = TeamRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        invitation.Status = InvitationStatus.Accepted;
        _db.TeamMembers.Add(member);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Invitation accepted" });
    }

    /// <summary>Declines a pending invitation.</summary>
    [HttpPost("invitations/{id:guid}/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeclineInvitation(Guid id)
    {
        var user = GetCurrentUser();

        var invitation = await _db.TeamInvitations
            .FirstOrDefaultAsync(i => i.Id == id
                && i.Email == user.Email
                && i.Status == InvitationStatus.Pending);

        if (invitation is null)
            return NotFound();

        invitation.Status = InvitationStatus.Declined;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Invitation declined" });
    }

    /// <summary>Removes a member from a team. Owner/Admin only. Cannot remove the owner.</summary>
    [HttpDelete("{teamId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid memberId)
    {
        var userId = GetCurrentUser().Id;

        var currentMembership = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (currentMembership is null)
            return NotFound();

        if (currentMembership.Role != TeamRole.Owner && currentMembership.Role != TeamRole.Admin)
            return StatusCode(StatusCodes.Status403Forbidden);

        var targetMember = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.Id == memberId && tm.TeamId == teamId);

        if (targetMember is null)
            return NotFound();

        if (targetMember.Role == TeamRole.Owner)
            return StatusCode(StatusCodes.Status403Forbidden);

        _db.TeamMembers.Remove(targetMember);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
