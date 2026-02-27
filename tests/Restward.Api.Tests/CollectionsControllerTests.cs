using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Restward.Api.Tests;

public class CollectionsControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public CollectionsControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoCollections()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/collections");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var collections = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions);
        Assert.NotNull(collections);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithValidInput()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/collections", new
        {
            name = "Test Collection",
            description = "A test collection",
            is_shared = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Test Collection", doc.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Create_ThenGetAll_IncludesNewCollection()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/collections", new
        {
            name = "Detailed Collection",
            description = "With details"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var getResponse = await client.GetAsync("/api/collections");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var body = await getResponse.Content.ReadAsStringAsync();
        var collections = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions)!;
        Assert.Contains(collections, c => c.GetProperty("id").GetString() == id);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithValidInput()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/collections", new
        {
            name = "Before Update"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var updateResponse = await client.PutAsJsonAsync($"/api/collections/{id}", new
        {
            name = "After Update"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("After Update", doc.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/collections", new
        {
            name = "To Delete"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/collections/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's no longer in the list
        var listResponse = await client.GetAsync("/api/collections");
        var body = await listResponse.Content.ReadAsStringAsync();
        var collections = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions)!;
        Assert.DoesNotContain(collections, c => c.GetProperty("id").GetString() == id);
    }

    [Fact]
    public async Task Request_Without_ApiKey_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/collections");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidInput_Returns400()
    {
        using var client = _factory.CreateAuthenticatedClient();

        // Name exceeds MaxLength(200)
        var response = await client.PostAsJsonAsync("/api/collections", new
        {
            name = new string('x', 201),
            description = "test"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
