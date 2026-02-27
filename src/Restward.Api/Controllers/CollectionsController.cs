using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api/collections")]
[EnableRateLimiting("standard")]
public class CollectionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CollectionsController(AppDbContext db)
    {
        _db = db;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    [HttpGet]
    public async Task<ActionResult<List<CollectionDto>>> GetAll()
    {
        var userId = GetCurrentUser().Id;

        var collections = await _db.Collections
            .Where(c => c.OwnerId == userId || c.IsShared)
            .Select(c => new CollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsShared = c.IsShared,
                OwnerId = c.OwnerId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(collections);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionDto>> GetById(Guid id)
    {
        var userId = GetCurrentUser().Id;

        var collection = await _db.Collections
            .Include(c => c.Folders.Where(f => f.ParentFolderId == null).OrderBy(f => f.SortOrder))
                .ThenInclude(f => f.SubFolders.OrderBy(sf => sf.SortOrder))
            .Include(c => c.Folders)
                .ThenInclude(f => f.Requests.OrderBy(r => r.SortOrder))
                    .ThenInclude(r => r.Headers)
            .Include(c => c.Folders)
                .ThenInclude(f => f.Requests)
                    .ThenInclude(r => r.Parameters)
            .Include(c => c.Requests.Where(r => r.FolderId == null).OrderBy(r => r.SortOrder))
                .ThenInclude(r => r.Headers)
            .Include(c => c.Requests.Where(r => r.FolderId == null))
                .ThenInclude(r => r.Parameters)
            .FirstOrDefaultAsync(c => c.Id == id && (c.OwnerId == userId || c.IsShared));

        if (collection is null)
            return NotFound();

        return Ok(MapCollection(collection));
    }

    [HttpPost]
    public async Task<ActionResult<CollectionDto>> Create([FromBody] CreateCollectionDto dto)
    {
        var user = GetCurrentUser();

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IsShared = dto.IsShared,
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Collections.Add(collection);
        await _db.SaveChangesAsync();

        return Created($"/api/collections/{collection.Id}", new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            IsShared = collection.IsShared,
            OwnerId = collection.OwnerId,
            CreatedAt = collection.CreatedAt,
            UpdatedAt = collection.UpdatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionDto>> Update(Guid id, [FromBody] UpdateCollectionDto dto)
    {
        var userId = GetCurrentUser().Id;
        var collection = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

        if (collection is null)
            return NotFound();

        if (dto.Name is not null) collection.Name = dto.Name;
        if (dto.Description is not null) collection.Description = dto.Description;
        if (dto.IsShared.HasValue) collection.IsShared = dto.IsShared.Value;
        collection.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            IsShared = collection.IsShared,
            OwnerId = collection.OwnerId,
            CreatedAt = collection.CreatedAt,
            UpdatedAt = collection.UpdatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUser().Id;
        var collection = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

        if (collection is null)
            return NotFound();

        _db.Collections.Remove(collection);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CollectionDto MapCollection(Collection c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        IsShared = c.IsShared,
        OwnerId = c.OwnerId,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Folders = c.Folders.Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.SortOrder)
            .Select(MapFolder)
            .ToList(),
        Requests = c.Requests.Where(r => r.FolderId == null)
            .OrderBy(r => r.SortOrder)
            .Select(MapRequest)
            .ToList()
    };

    internal static FolderDto MapFolder(Folder f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        CollectionId = f.CollectionId,
        ParentFolderId = f.ParentFolderId,
        SortOrder = f.SortOrder,
        SubFolders = f.SubFolders.OrderBy(sf => sf.SortOrder).Select(MapFolder).ToList(),
        Requests = f.Requests.OrderBy(r => r.SortOrder).Select(MapRequest).ToList()
    };

    internal static RequestDto MapRequest(RequestItem r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Method = r.Method,
        Url = r.Url,
        Body = r.Body,
        BodyContentType = r.BodyContentType,
        CollectionId = r.CollectionId,
        FolderId = r.FolderId,
        SortOrder = r.SortOrder,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Headers = r.Headers.Select(h => new KeyValuePairDto
        {
            Id = h.Id, Key = h.Key, Value = h.Value, Enabled = h.Enabled
        }).ToList(),
        Parameters = r.Parameters.Select(p => new KeyValuePairDto
        {
            Id = p.Id, Key = p.Key, Value = p.Value, Enabled = p.Enabled
        }).ToList()
    };
}
