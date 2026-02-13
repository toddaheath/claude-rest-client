using System.Text.Json.Serialization;

namespace ClaudeRestClient.Models;

public class Message
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}
