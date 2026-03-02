using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

            // Remove existing rate limiter configuration and re-add with higher limits for testing
            var rateLimiterDescriptors = services
                .Where(d => d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>))
                .ToList();
            foreach (var rlDescriptor in rateLimiterDescriptors)
                services.Remove(rlDescriptor);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                options.AddFixedWindowLimiter("standard", opt =>
                {
                    opt.PermitLimit = 10000;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("proxy", opt =>
                {
                    opt.PermitLimit = 10000;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.PermitLimit = 10000;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });
            });
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-jwt-secret-key-for-integration-tests-min-32",
                ["Jwt:Issuer"] = "Restward",
                ["Jwt:Audience"] = "Restward",
                ["Jwt:ExpirationMinutes"] = "60"
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
