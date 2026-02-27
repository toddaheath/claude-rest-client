using System.Net;
using System.Text.Json;

namespace Restward.Api.Tests;

public class SwaggerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;

    public SwaggerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_ContainsApiInfo()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("openapi", out var openapi));
        Assert.StartsWith("3.", openapi.GetString());

        var info = root.GetProperty("info");
        Assert.Equal("Restward API", info.GetProperty("title").GetString());
        Assert.Equal("v1", info.GetProperty("version").GetString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsAllPaths()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var paths = doc.RootElement.GetProperty("paths");

        var expectedPaths = new[]
        {
            "/api/users/me",
            "/api/users",
            "/api/users/{id}",
            "/api/collections",
            "/api/collections/{id}",
            "/api/proxy",
            "/api/collections/{collectionId}/requests",
            "/api/requests/{id}",
            "/api/collections/{collectionId}/folders",
            "/api/folders/{id}",
            "/api/import/postman",
            "/api/import/insomnia",
            "/api/export/postman/{id}",
            "/api/export/insomnia/{id}",
            "/api/environments",
            "/api/environments/{id}",
        };

        foreach (var path in expectedPaths)
        {
            Assert.True(paths.TryGetProperty(path, out _), $"Missing path: {path}");
        }
    }

    [Fact]
    public async Task SwaggerJson_DefinesApiKeySecurity()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // OpenAPI 3.x uses components.securitySchemes
        var securitySchemes = root
            .GetProperty("components")
            .GetProperty("securitySchemes");

        Assert.True(securitySchemes.TryGetProperty("ApiKey", out var apiKeyScheme));
        Assert.Equal("apiKey", apiKeyScheme.GetProperty("type").GetString());
        Assert.Equal("header", apiKeyScheme.GetProperty("in").GetString());
        Assert.Equal("X-Api-Key", apiKeyScheme.GetProperty("name").GetString());
    }

    [Fact]
    public async Task SwaggerUI_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("text/html", contentType);
    }

    [Fact]
    public async Task SwaggerEndpoints_DoNotRequireAuth()
    {
        using var client = _factory.CreateClient();
        // No X-Api-Key header set

        var jsonResponse = await client.GetAsync("/swagger/v1/swagger.json");
        var uiResponse = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, jsonResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
    }
}
