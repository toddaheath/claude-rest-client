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

    [HttpGet("me")]
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

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        if (!GetCurrentUser().IsAdmin)
            return Forbid();

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

    [HttpPost]
    public async Task<ActionResult<UserWithKeyDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!GetCurrentUser().IsAdmin)
            return Forbid();

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!GetCurrentUser().IsAdmin)
            return Forbid();

        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        _cache.Remove($"auth:{user.ApiKey}");
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
