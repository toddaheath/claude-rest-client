using System.Net.Http.Json;
using System.Text.Json;
using ClaudeRestClient.Models;

namespace ClaudeRestClient;

public class ClaudeClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeClientOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeClient(ClaudeClientOptions options, HttpClient? httpClient = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? new HttpClient();

        _httpClient.BaseAddress ??= new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", _options.AnthropicVersion);
    }

    public async Task<MessageResponse> SendMessageAsync(MessageRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/v1/messages", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Claude API returned {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<MessageResponse>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Deserialized response was null.");
    }
}
