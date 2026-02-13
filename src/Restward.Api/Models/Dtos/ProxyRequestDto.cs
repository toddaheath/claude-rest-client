using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class ProxyRequestDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("body_content_type")]
    public string? BodyContentType { get; set; }
}
