using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Restward.Api.Tests;

public class FoldersControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public FoldersControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> CreateCollectionAsync(HttpClient client, string name = "Test Collection")
    {
        var response = await client.PostAsJsonAsync("/api/collections", new
        {
            name,
            description = "For folder tests"
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

        var response = await client.PostAsJsonAsync($"/api/collections/{collectionId}/folders", new
        {
            name = "Auth"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var folder = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Auth", folder.GetProperty("name").GetString());
        Assert.Equal(collectionId, folder.GetProperty("collection_id").GetString());
    }

    [Fact]
    public async Task Update_ChangesFolderName()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var createResponse = await client.PostAsJsonAsync($"/api/collections/{collectionId}/folders", new
        {
            name = "Before Update"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var folderId = created.GetProperty("id").GetString();

        var updateResponse = await client.PutAsJsonAsync($"/api/folders/{folderId}", new
        {
            name = "After Update"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadAsStringAsync();
        var folder = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("After Update", folder.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var collectionId = await CreateCollectionAsync(client);

        var createResponse = await client.PostAsJsonAsync($"/api/collections/{collectionId}/folders", new
        {
            name = "To Delete"
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var folderId = created.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/folders/{folderId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_InNonExistentCollection_Returns404()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var fakeId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync($"/api/collections/{fakeId}/folders", new
        {
            name = "Orphan Folder"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistentFolder_Returns404()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var fakeId = Guid.NewGuid();

        var response = await client.PutAsJsonAsync($"/api/folders/{fakeId}", new
        {
            name = "Does Not Exist"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentFolder_Returns404()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var fakeId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/folders/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
