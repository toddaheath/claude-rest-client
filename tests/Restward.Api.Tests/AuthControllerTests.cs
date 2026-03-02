using System.Net;
using System.Net.Http.Json;
using Restward.Api.Models.Dtos;

namespace Restward.Api.Tests;

public class AuthControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(RestwardWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidInput_ReturnsTokenAndUser()
    {
        var dto = new RegisterDto
        {
            Name = "Test User",
            Email = $"test-{Guid.NewGuid():N}@example.com",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal(dto.Name, result.User.Name);
        Assert.Equal(dto.Email, result.User.Email);
        Assert.False(string.IsNullOrEmpty(result.ApiKey));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var dto = new RegisterDto { Name = "User1", Email = email, Password = "password123" };

        await _client.PostAsJsonAsync("/api/auth/register", dto);
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Name = "User2", Email = email, Password = "password456" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Name = "Login User", Email = email, Password = "password123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var email = $"bad-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Name = "Bad Login", Email = email, Password = "password123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "wrongpassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "nonexistent@example.com", Password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JwtToken_CanAccessProtectedEndpoint()
    {
        var email = $"jwt-{Guid.NewGuid():N}@example.com";
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Name = "JWT User", Email = email, Password = "password123" });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Add("Authorization", $"Bearer {auth!.Token}");

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task NoAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
