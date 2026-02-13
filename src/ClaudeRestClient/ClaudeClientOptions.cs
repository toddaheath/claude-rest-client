namespace ClaudeRestClient;

public class ClaudeClientOptions
{
    public required string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string Model { get; set; } = "claude-sonnet-4-5-20250929";
    public int MaxTokens { get; set; } = 1024;
    public string AnthropicVersion { get; set; } = "2023-06-01";
}
