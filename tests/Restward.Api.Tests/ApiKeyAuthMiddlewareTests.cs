using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restward.Api.Data;
using Restward.Api.Middleware;
using Restward.Api.Models.Entities;

namespace Restward.Api.Tests;

public class ApiKeyAuthMiddlewareTests
{
    private static (AppDbContext Db, ApiKeyAuthMiddleware Middleware) CreateTestSetup(RequestDelegate next)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        var middleware = new ApiKeyAuthMiddleware(next);
        return (db, middleware);
    }

    private static HttpContext CreateHttpContext(AppDbContext db, string? apiKey = null, string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        context.RequestServices = services.BuildServiceProvider();

        if (apiKey is not null)
            context.Request.Headers["X-Api-Key"] = apiKey;

        return context;
    }

    [Fact]
    public async Task HealthEndpoint_SkipsAuth()
    {
        var nextCalled = false;
        var (db, middleware) = CreateTestSetup(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateHttpContext(db, path: "/health");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task MissingApiKey_Returns401()
    {
        var (db, middleware) = CreateTestSetup(_ => Task.CompletedTask);
        var context = CreateHttpContext(db);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvalidApiKey_Returns401()
    {
        var (db, middleware) = CreateTestSetup(_ => Task.CompletedTask);
        var context = CreateHttpContext(db, apiKey: "invalid-key");

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task ValidApiKey_SetsUserAndCallsNext()
    {
        var nextCalled = false;
        var (db, middleware) = CreateTestSetup(_ => { nextCalled = true; return Task.CompletedTask; });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "TestUser",
            ApiKey = "test-api-key-123",
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var context = CreateHttpContext(db, apiKey: "test-api-key-123");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.NotNull(context.Items["User"]);
        Assert.Equal(user.Id, ((User)context.Items["User"]!).Id);
    }
}
