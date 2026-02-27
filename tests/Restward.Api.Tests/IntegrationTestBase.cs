using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restward.Api.Data;
using Restward.Api.Models.Entities;

namespace Restward.Api.Tests;

public class RestwardWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "test-api-key-for-integration-tests";
    private readonly string _dbName = "RestwardTestDb_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed test user after app startup
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!db.Users.Any(u => u.ApiKey == TestApiKey))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser",
                ApiKey = TestApiKey,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        return host;
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
        return client;
    }
}
