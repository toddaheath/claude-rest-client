using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class ProxyResponseDto
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("status_text")]
    public string StatusText { get; set; } = string.Empty;

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = [];

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("elapsed_ms")]
    public long ElapsedMs { get; set; }

    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; set; }
}
