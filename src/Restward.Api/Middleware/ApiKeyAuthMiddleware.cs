using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Restward.Api.Data;
using Restward.Api.Models.Entities;
using Restward.Api.Services;

namespace Restward.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-Api-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
        var db = context.RequestServices.GetRequiredService<AppDbContext>();

        // Try API key first
        if (context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey) &&
            !string.IsNullOrWhiteSpace(apiKey))
        {
            var cacheKey = $"auth:{apiKey}";
            var user = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey.ToString());
            });

            if (user is not null)
            {
                context.Items["User"] = user;
                await _next(context);
                return;
            }

            cache.Remove(cacheKey);
        }

        // Try JWT Bearer token
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader is not null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var jwtService = context.RequestServices.GetRequiredService<JwtService>();
            var principal = jwtService.ValidateToken(token);

            if (principal is not null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var jwtCacheKey = $"auth:jwt:{userId}";
                    var user = await cache.GetOrCreateAsync(jwtCacheKey, async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                        return await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    });

                    if (user is not null)
                    {
                        context.Items["User"] = user;
                        await _next(context);
                        return;
                    }

                    cache.Remove(jwtCacheKey);
                }
            }
        }

        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Authentication required. Provide X-Api-Key header or Authorization: Bearer <token>" });
    }
}
