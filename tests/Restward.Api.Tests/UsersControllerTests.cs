using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Restward.Api.Tests;

public class UsersControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public UsersControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_ReturnsCurrentUser()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("TestUser", user.GetProperty("name").GetString());
        Assert.True(user.GetProperty("is_admin").GetBoolean());
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsUserList()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions);
        Assert.NotNull(users);
        Assert.True(users.Length >= 1);
    }

    [Fact]
    public async Task Create_AsAdmin_ReturnsCreatedWithApiKey()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            name = "NewUser",
            is_admin = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("NewUser", user.GetProperty("name").GetString());
        Assert.False(user.GetProperty("is_admin").GetBoolean());
        Assert.False(string.IsNullOrEmpty(user.GetProperty("api_key").GetString()));
    }

    [Fact]
    public async Task Delete_AsAdmin_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();

        // Create a user to delete
        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            name = "ToDelete",
            is_admin = false
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/users/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsNonAdmin_Returns403()
    {
        // Create a non-admin user via the admin client
        using var adminClient = _factory.CreateAuthenticatedClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/users", new
        {
            name = "RegularUser",
            is_admin = false
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var apiKey = created.GetProperty("api_key").GetString();

        // Use the non-admin key
        using var nonAdminClient = _factory.CreateClient();
        nonAdminClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await nonAdminClient.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsNonAdmin_Returns403()
    {
        using var adminClient = _factory.CreateAuthenticatedClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/users", new
        {
            name = "RegularUser2",
            is_admin = false
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var apiKey = created.GetProperty("api_key").GetString();

        using var nonAdminClient = _factory.CreateClient();
        nonAdminClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await nonAdminClient.PostAsJsonAsync("/api/users", new
        {
            name = "ShouldFail",
            is_admin = false
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsNonAdmin_Returns403()
    {
        using var adminClient = _factory.CreateAuthenticatedClient();
        var createResponse = await adminClient.PostAsJsonAsync("/api/users", new
        {
            name = "RegularUser3",
            is_admin = false
        });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var apiKey = created.GetProperty("api_key").GetString();
        var id = created.GetProperty("id").GetString();

        using var nonAdminClient = _factory.CreateClient();
        nonAdminClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await nonAdminClient.DeleteAsync($"/api/users/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
