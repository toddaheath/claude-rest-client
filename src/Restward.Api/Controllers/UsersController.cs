using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/users")]
[EnableRateLimiting("standard")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public UsersController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    /// <summary>Gets the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserDto> GetMe()
    {
        var user = GetCurrentUser();
        return Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>Lists all users. Requires admin privileges.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        if (!GetCurrentUser().IsAdmin)
            return StatusCode(403);

        var users = await _db.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                IsAdmin = u.IsAdmin,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>Creates a new user. Requires admin privileges.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserWithKeyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserWithKeyDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!GetCurrentUser().IsAdmin)
            return StatusCode(403);

        var apiKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            IsAdmin = dto.IsAdmin,
            ApiKey = apiKey,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{user.Id}", new UserWithKeyDto
        {
            Id = user.Id,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            ApiKey = user.ApiKey,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>Deletes a user by ID. Requires admin privileges.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!GetCurrentUser().IsAdmin)
            return StatusCode(403);

        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        _cache.Remove($"auth:{user.ApiKey}");
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
