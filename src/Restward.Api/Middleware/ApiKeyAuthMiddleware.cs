using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Restward.Api.Data;
using Restward.Api.Models.Entities;

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
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey) ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing X-Api-Key header" });
            return;
        }

        var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
        var cacheKey = $"auth:{apiKey}";

        var user = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            var db = context.RequestServices.GetRequiredService<AppDbContext>();
            return await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey.ToString());
        });

        if (user is null)
        {
            cache.Remove(cacheKey);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        context.Items["User"] = user;
        await _next(context);
    }
}
