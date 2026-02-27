using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Restward.Api.Tests;

public class RequestsControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public RequestsControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> CreateCollectionAsync(HttpClient client, string name = "Test Collection")
    {
        var response = await client.PostAsJsonAsync("/api/collections", new
        {
            name,
            description = "For request tests"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        return doc.GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task Create_ReturnsCreated_InCollection()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var response = await client.PostAsJsonAsync($"/api/collections/{collectionId}/requests", new
        {
            name = "Get Users",
            method = "GET",
            url = "https://api.example.com/users",
            headers = new[]
            {
                new { key = "Accept", value = "application/json", enabled = true }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Get Users", request.GetProperty("name").GetString());
        Assert.Equal("GET", request.GetProperty("method").GetString());
        Assert.Equal(collectionId, request.GetProperty("collection_id").GetString());
    }

    [Fact]
    public async Task Update_ChangesRequestProperties()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var createResponse = await client.PostAsJsonAsync($"/api/collections/{collectionId}/requests", new
        {
            name = "Before Update",
            method = "GET",
            url = "https://api.example.com/old"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var requestId = created.GetProperty("id").GetString();

        var updateResponse = await client.PutAsJsonAsync($"/api/requests/{requestId}", new
        {
            name = "After Update",
            method = "POST",
            url = "https://api.example.com/new",
            body = "{\"key\":\"value\"}",
            body_content_type = "application/json"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("After Update", request.GetProperty("name").GetString());
        Assert.Equal("POST", request.GetProperty("method").GetString());
        Assert.Equal("https://api.example.com/new", request.GetProperty("url").GetString());
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var createResponse = await client.PostAsJsonAsync($"/api/collections/{collectionId}/requests", new
        {
            name = "To Delete",
            method = "GET",
            url = "https://api.example.com/delete"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var requestId = created.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/requests/{requestId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/requests/{requestId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_InNonExistentCollection_Returns404()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var fakeId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync($"/api/collections/{fakeId}/requests", new
        {
            name = "Orphan Request",
            method = "GET",
            url = "https://api.example.com/orphan"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithHeaders_AndParameters()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var response = await client.PostAsJsonAsync($"/api/collections/{collectionId}/requests", new
        {
            name = "Full Request",
            method = "POST",
            url = "https://api.example.com/data",
            body = "{\"test\":true}",
            body_content_type = "application/json",
            headers = new[]
            {
                new { key = "Authorization", value = "Bearer token", enabled = true },
                new { key = "Content-Type", value = "application/json", enabled = true }
            },
            parameters = new[]
            {
                new { key = "page", value = "1", enabled = true },
                new { key = "limit", value = "10", enabled = true }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal(2, request.GetProperty("headers").GetArrayLength());
        Assert.Equal(2, request.GetProperty("parameters").GetArrayLength());
    }
}
