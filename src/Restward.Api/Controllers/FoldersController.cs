using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;

namespace Restward.Api.Controllers;

[ApiController]
[EnableRateLimiting("standard")]
public class FoldersController : ControllerBase
{
    private readonly AppDbContext _db;

    public FoldersController(AppDbContext db)
    {
        _db = db;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    [HttpPost("api/collections/{collectionId:guid}/folders")]
    public async Task<ActionResult<FolderDto>> Create(Guid collectionId, [FromBody] CreateFolderDto dto)
    {
        var userId = GetCurrentUser().Id;
        var collection = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && (c.OwnerId == userId || c.IsShared));

        if (collection is null)
            return NotFound();

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CollectionId = collectionId,
            ParentFolderId = dto.ParentFolderId,
            SortOrder = dto.SortOrder
        };

        _db.Folders.Add(folder);
        await _db.SaveChangesAsync();

        return Created($"/api/folders/{folder.Id}", new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            CollectionId = folder.CollectionId,
            ParentFolderId = folder.ParentFolderId,
            SortOrder = folder.SortOrder
        });
    }

    [HttpPut("api/folders/{id:guid}")]
    public async Task<ActionResult<FolderDto>> Update(Guid id, [FromBody] UpdateFolderDto dto)
    {
        var userId = GetCurrentUser().Id;
        var folder = await _db.Folders
            .Include(f => f.Collection)
            .FirstOrDefaultAsync(f => f.Id == id && (f.Collection.OwnerId == userId || f.Collection.IsShared));

        if (folder is null)
            return NotFound();

        if (dto.Name is not null) folder.Name = dto.Name;
        if (dto.ParentFolderId.HasValue) folder.ParentFolderId = dto.ParentFolderId;
        if (dto.SortOrder.HasValue) folder.SortOrder = dto.SortOrder.Value;

        await _db.SaveChangesAsync();

        return Ok(new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            CollectionId = folder.CollectionId,
            ParentFolderId = folder.ParentFolderId,
            SortOrder = folder.SortOrder
        });
    }

    [HttpDelete("api/folders/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUser().Id;
        var folder = await _db.Folders
            .Include(f => f.Collection)
            .FirstOrDefaultAsync(f => f.Id == id && (f.Collection.OwnerId == userId || f.Collection.IsShared));

        if (folder is null)
            return NotFound();

        _db.Folders.Remove(folder);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
