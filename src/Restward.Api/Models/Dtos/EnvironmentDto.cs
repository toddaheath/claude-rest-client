using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class EnvironmentDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_shared")]
    public bool IsShared { get; set; }

    [JsonPropertyName("owner_id")]
    public Guid OwnerId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("variables")]
    public List<KeyValuePairDto> Variables { get; set; } = [];
}

public class CreateEnvironmentDto
{
    [Required]
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_shared")]
    public bool IsShared { get; set; }

    [JsonPropertyName("variables")]
    public List<KeyValuePairDto>? Variables { get; set; }
}

public class UpdateEnvironmentDto
{
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("is_shared")]
    public bool? IsShared { get; set; }

    [JsonPropertyName("variables")]
    public List<KeyValuePairDto>? Variables { get; set; }
}
