using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Models.Dtos;
using Restward.Api.Models.Entities;

namespace Restward.Api.Controllers;

[ApiController]
[EnableRateLimiting("standard")]
public class RequestsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RequestsController(AppDbContext db)
    {
        _db = db;
    }

    private User GetCurrentUser() => (User)HttpContext.Items["User"]!;

    /// <summary>Creates a new request within a collection.</summary>
    [HttpPost("api/collections/{collectionId:guid}/requests")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RequestDto>> Create(Guid collectionId, [FromBody] CreateRequestDto dto)
    {
        var userId = GetCurrentUser().Id;
        var collection = await _db.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && (c.OwnerId == userId || c.IsShared));

        if (collection is null)
            return NotFound();

        var request = new RequestItem
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Method = dto.Method,
            Url = dto.Url,
            Body = dto.Body,
            BodyContentType = dto.BodyContentType,
            CollectionId = collectionId,
            FolderId = dto.FolderId,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Headers = dto.Headers?.Select(h => new RequestHeader
            {
                Id = Guid.NewGuid(),
                Key = h.Key,
                Value = h.Value,
                Enabled = h.Enabled
            }).ToList() ?? [],
            Parameters = dto.Parameters?.Select(p => new RequestParameter
            {
                Id = Guid.NewGuid(),
                Key = p.Key,
                Value = p.Value,
                Enabled = p.Enabled
            }).ToList() ?? []
        };

        _db.RequestItems.Add(request);
        await _db.SaveChangesAsync();

        return Created($"/api/requests/{request.Id}", CollectionsController.MapRequest(request));
    }

    /// <summary>Gets a request by ID.</summary>
    [HttpGet("api/requests/{id:guid}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RequestDto>> GetById(Guid id)
    {
        var userId = GetCurrentUser().Id;
        var request = await _db.RequestItems
            .Include(r => r.Headers)
            .Include(r => r.Parameters)
            .Include(r => r.Collection)
            .FirstOrDefaultAsync(r => r.Id == id && (r.Collection.OwnerId == userId || r.Collection.IsShared));

        if (request is null)
            return NotFound();

        return Ok(CollectionsController.MapRequest(request));
    }

    /// <summary>Updates an existing request.</summary>
    [HttpPut("api/requests/{id:guid}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RequestDto>> Update(Guid id, [FromBody] UpdateRequestDto dto)
    {
        var userId = GetCurrentUser().Id;
        var request = await _db.RequestItems
            .Include(r => r.Headers)
            .Include(r => r.Parameters)
            .Include(r => r.Collection)
            .FirstOrDefaultAsync(r => r.Id == id && (r.Collection.OwnerId == userId || r.Collection.IsShared));

        if (request is null)
            return NotFound();

        if (dto.Name is not null) request.Name = dto.Name;
        if (dto.Method is not null) request.Method = dto.Method;
        if (dto.Url is not null) request.Url = dto.Url;
        if (dto.Body is not null) request.Body = dto.Body;
        if (dto.BodyContentType is not null) request.BodyContentType = dto.BodyContentType;
        if (dto.FolderId.HasValue) request.FolderId = dto.FolderId;
        if (dto.SortOrder.HasValue) request.SortOrder = dto.SortOrder.Value;
        request.UpdatedAt = DateTime.UtcNow;

        if (dto.Headers is not null)
        {
            _db.RequestHeaders.RemoveRange(request.Headers);
            request.Headers = dto.Headers.Select(h => new RequestHeader
            {
                Id = Guid.NewGuid(),
                Key = h.Key,
                Value = h.Value,
                Enabled = h.Enabled,
                RequestItemId = request.Id
            }).ToList();
        }

        if (dto.Parameters is not null)
        {
            _db.RequestParameters.RemoveRange(request.Parameters);
            request.Parameters = dto.Parameters.Select(p => new RequestParameter
            {
                Id = Guid.NewGuid(),
                Key = p.Key,
                Value = p.Value,
                Enabled = p.Enabled,
                RequestItemId = request.Id
            }).ToList();
        }

        await _db.SaveChangesAsync();
        return Ok(CollectionsController.MapRequest(request));
    }

    /// <summary>Deletes a request by ID.</summary>
    [HttpDelete("api/requests/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUser().Id;
        var request = await _db.RequestItems
            .Include(r => r.Collection)
            .FirstOrDefaultAsync(r => r.Id == id && (r.Collection.OwnerId == userId || r.Collection.IsShared));

        if (request is null)
            return NotFound();

        _db.RequestItems.Remove(request);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
