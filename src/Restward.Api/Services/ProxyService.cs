using System.Diagnostics;
using System.Text;
using Restward.Api.Models.Dtos;

namespace Restward.Api.Services;

public class ProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProxyService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ProxyResponseDto> ExecuteAsync(ProxyRequestDto request)
    {
        var client = _httpClientFactory.CreateClient("Proxy");
        var httpMethod = new HttpMethod(request.Method.ToUpperInvariant());

        using var httpRequest = new HttpRequestMessage(httpMethod, request.Url);

        if (request.Headers is not null)
        {
            foreach (var (key, value) in request.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (request.Body is not null && httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head)
        {
            var contentType = request.BodyContentType ?? "application/json";
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
        }

        var stopwatch = Stopwatch.StartNew();
        using var httpResponse = await client.SendAsync(httpRequest);
        stopwatch.Stop();

        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        var responseHeaders = new Dictionary<string, string>();

        foreach (var header in httpResponse.Headers)
        {
            responseHeaders[header.Key] = string.Join(", ", header.Value);
        }
        foreach (var header in httpResponse.Content.Headers)
        {
            responseHeaders[header.Key] = string.Join(", ", header.Value);
        }

        return new ProxyResponseDto
        {
            StatusCode = (int)httpResponse.StatusCode,
            StatusText = httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString(),
            Headers = responseHeaders,
            Body = responseBody,
            ElapsedMs = stopwatch.ElapsedMilliseconds,
            SizeBytes = Encoding.UTF8.GetByteCount(responseBody)
        };
    }
}
