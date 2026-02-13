using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;
using Environment = Restward.Api.Models.Entities.Environment;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/environments")]
public class EnvironmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EnvironmentsController(AppDbContext db)
    {
        _db = db;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    [HttpGet]
    public async Task<ActionResult<List<EnvironmentDto>>> GetAll()
    {
        var userId = GetCurrentUser().Id;

        var environments = await _db.Environments
            .Include(e => e.Variables)
            .Where(e => e.OwnerId == userId || e.IsShared)
            .Select(e => new EnvironmentDto
            {
                Id = e.Id,
                Name = e.Name,
                IsShared = e.IsShared,
                OwnerId = e.OwnerId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Variables = e.Variables.Select(v => new KeyValuePairDto
                {
                    Id = v.Id, Key = v.Key, Value = v.Value, Enabled = v.Enabled
                }).ToList()
            })
            .ToListAsync();

        return Ok(environments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EnvironmentDto>> GetById(Guid id)
    {
        var userId = GetCurrentUser().Id;

        var env = await _db.Environments
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == id && (e.OwnerId == userId || e.IsShared));

        if (env is null)
            return NotFound();

        return Ok(MapEnvironment(env));
    }

    [HttpPost]
    public async Task<ActionResult<EnvironmentDto>> Create([FromBody] CreateEnvironmentDto dto)
    {
        var user = GetCurrentUser();

        var env = new Environment
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            IsShared = dto.IsShared,
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Variables = dto.Variables?.Select(v => new EnvironmentVariable
            {
                Id = Guid.NewGuid(),
                Key = v.Key,
                Value = v.Value,
                Enabled = v.Enabled
            }).ToList() ?? []
        };

        _db.Environments.Add(env);
        await _db.SaveChangesAsync();

        return Created($"/api/environments/{env.Id}", MapEnvironment(env));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EnvironmentDto>> Update(Guid id, [FromBody] UpdateEnvironmentDto dto)
    {
        var userId = GetCurrentUser().Id;
        var env = await _db.Environments
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

        if (env is null)
            return NotFound();

        if (dto.Name is not null) env.Name = dto.Name;
        if (dto.IsShared.HasValue) env.IsShared = dto.IsShared.Value;
        env.UpdatedAt = DateTime.UtcNow;

        if (dto.Variables is not null)
        {
            _db.EnvironmentVariables.RemoveRange(env.Variables);
            env.Variables = dto.Variables.Select(v => new EnvironmentVariable
            {
                Id = Guid.NewGuid(),
                Key = v.Key,
                Value = v.Value,
                Enabled = v.Enabled,
                EnvironmentId = env.Id
            }).ToList();
        }

        await _db.SaveChangesAsync();
        return Ok(MapEnvironment(env));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUser().Id;
        var env = await _db.Environments
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

        if (env is null)
            return NotFound();

        _db.Environments.Remove(env);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static EnvironmentDto MapEnvironment(Environment e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        IsShared = e.IsShared,
        OwnerId = e.OwnerId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Variables = e.Variables.Select(v => new KeyValuePairDto
        {
            Id = v.Id, Key = v.Key, Value = v.Value, Enabled = v.Enabled
        }).ToList()
    };
}
