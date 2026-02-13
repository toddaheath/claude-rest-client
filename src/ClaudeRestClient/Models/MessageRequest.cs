using System.Text.Json.Serialization;

namespace ClaudeRestClient.Models;

public class MessageRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public required List<Message> Messages { get; set; }

    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }
}
