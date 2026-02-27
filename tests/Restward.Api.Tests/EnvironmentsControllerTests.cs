using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Restward.Api.Tests;

public class EnvironmentsControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public EnvironmentsControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/environments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var environments = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions);
        Assert.NotNull(environments);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithVariables()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/environments", new
        {
            name = "Development",
            is_shared = false,
            variables = new[]
            {
                new { key = "BASE_URL", value = "http://localhost:3000", enabled = true },
                new { key = "API_KEY", value = "dev-key-123", enabled = true }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var env = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Development", env.GetProperty("name").GetString());
        Assert.Equal(2, env.GetProperty("variables").GetArrayLength());
    }

    [Fact]
    public async Task Create_ThenGetAll_IncludesNewEnvironment()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/environments", new
        {
            name = "Staging"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var getResponse = await client.GetAsync("/api/environments");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var body = await getResponse.Content.ReadAsStringAsync();
        var environments = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions)!;
        Assert.Contains(environments, e => e.GetProperty("id").GetString() == id);
    }

    [Fact]
    public async Task Update_ChangesName()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/environments", new
        {
            name = "Before Update"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var updateResponse = await client.PutAsJsonAsync($"/api/environments/{id}", new
        {
            name = "After Update"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadAsStringAsync();
        var env = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("After Update", env.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/environments", new
        {
            name = "To Delete"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/environments/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/environments/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/environments", new
        {
            name = "",
            is_shared = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNameExceedingMaxLength_Returns400()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/environments", new
        {
            name = new string('x', 201)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
