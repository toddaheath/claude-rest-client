using System.Net;
using System.Net.Http.Json;

namespace Restward.Api.Tests;

public class ProxyControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;

    public ProxyControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Execute_WithEmptyUrl_Returns400()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/proxy", new
        {
            method = "GET",
            url = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Execute_WithNonHttpUrl_Returns400()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/proxy", new
        {
            method = "GET",
            url = "ftp://example.com/file"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Execute_WithLocalhostUrl_Returns400_SsrfBlocked()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/proxy", new
        {
            method = "GET",
            url = "http://127.0.0.1/admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("private", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Execute_WithPrivateIpUrl_Returns400_SsrfBlocked()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/proxy", new
        {
            method = "GET",
            url = "http://10.0.0.1/internal"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("private", body, StringComparison.OrdinalIgnoreCase);
    }
}
