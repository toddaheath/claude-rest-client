using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Restward.Api.Data;
using Restward.Api.Middleware;
using Restward.Api.Models.Entities;
using Restward.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("Proxy")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 10
    });

builder.Services.AddSingleton<ProxyService>();
builder.Services.AddSingleton<ImportService>();
builder.Services.AddSingleton<ExportService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync(u => u.IsAdmin))
    {
        var adminKey = builder.Configuration["AdminApiKey"]
            ?? Convert.ToHexString(RandomNumberGenerator.GetBytes(24));

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            ApiKey = adminKey,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        app.Logger.LogInformation("Admin user created with API key: {ApiKey}", adminKey);
    }
}

app.UseCors();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
