using System.Net;
using Restward.Api.Models.Dtos;
using Restward.Api.Services;

namespace Restward.Api.Tests;

public class ProxyServiceTests
{
    private class FakeHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHandler(HttpResponseMessage response) => _response = response;
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }
    }

    private static (ProxyService Service, FakeHandler Handler) CreateService(HttpResponseMessage response)
    {
        var handler = new FakeHandler(response);
        var client = new HttpClient(handler);
        var factory = new TestHttpClientFactory(client);
        return (new ProxyService(factory), handler);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsStatusAndBody()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ok\":true}", System.Text.Encoding.UTF8, "application/json"),
            ReasonPhrase = "OK"
        };

        var (service, _) = CreateService(response);
        var result = await service.ExecuteAsync(new ProxyRequestDto
        {
            Method = "GET",
            Url = "https://api.example.com/test"
        });

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.StatusText);
        Assert.Contains("ok", result.Body);
        Assert.True(result.ElapsedMs >= 0);
        Assert.True(result.SizeBytes > 0);
    }

    [Fact]
    public async Task ExecuteAsync_SendsHeaders()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };

        var (service, handler) = CreateService(response);
        await service.ExecuteAsync(new ProxyRequestDto
        {
            Method = "GET",
            Url = "https://api.example.com/test",
            Headers = new Dictionary<string, string> { ["X-Custom"] = "value" }
        });

        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.Contains("X-Custom"));
    }

    [Fact]
    public async Task ExecuteAsync_SendsBody()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("")
        };

        var (service, handler) = CreateService(response);
        await service.ExecuteAsync(new ProxyRequestDto
        {
            Method = "POST",
            Url = "https://api.example.com/test",
            Body = "{\"key\":\"value\"}",
            BodyContentType = "application/json"
        });

        Assert.NotNull(handler.LastRequest?.Content);
    }

    private class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public TestHttpClientFactory(HttpClient client) => _client = client;
        public HttpClient CreateClient(string name) => _client;
    }
}
