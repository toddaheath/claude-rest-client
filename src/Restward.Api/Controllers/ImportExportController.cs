using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;
using Restward.Api.Services;

namespace Restward.Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("standard")]
public class ImportExportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ImportService _importService;
    private readonly ExportService _exportService;

    public ImportExportController(AppDbContext db, ImportService importService, ExportService exportService)
    {
        _db = db;
        _importService = importService;
        _exportService = exportService;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    [HttpPost("import/postman")]
    public async Task<ActionResult<CollectionDto>> ImportPostman()
    {
        var user = GetCurrentUser();

        using var doc = await JsonDocument.ParseAsync(Request.Body);
        var collection = _importService.ImportPostmanV2(doc, user.Id);

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

    [HttpPost("import/insomnia")]
    public async Task<ActionResult<CollectionDto>> ImportInsomnia()
    {
        var user = GetCurrentUser();

        using var doc = await JsonDocument.ParseAsync(Request.Body);
        var collection = _importService.ImportInsomniaV4(doc, user.Id);

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

    [HttpGet("export/postman/{id:guid}")]
    public async Task<IActionResult> ExportPostman(Guid id)
    {
        var collection = await LoadCollectionForExport(id);
        if (collection is null) return NotFound();

        var result = _exportService.ExportPostmanV2(collection);
        return Content(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            "application/json");
    }

    [HttpGet("export/insomnia/{id:guid}")]
    public async Task<IActionResult> ExportInsomnia(Guid id)
    {
        var collection = await LoadCollectionForExport(id);
        if (collection is null) return NotFound();

        var result = _exportService.ExportInsomniaV4(collection);
        return Content(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            "application/json");
    }

    private async Task<Collection?> LoadCollectionForExport(Guid id)
    {
        var userId = GetCurrentUser().Id;
        return await _db.Collections
            .Include(c => c.Folders)
            .Include(c => c.Requests).ThenInclude(r => r.Headers)
            .Include(c => c.Requests).ThenInclude(r => r.Parameters)
            .FirstOrDefaultAsync(c => c.Id == id && (c.OwnerId == userId || c.IsShared));
    }
}
