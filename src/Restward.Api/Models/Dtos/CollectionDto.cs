using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class CollectionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_shared")]
    public bool IsShared { get; set; }

    [JsonPropertyName("owner_id")]
    public Guid OwnerId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("folders")]
    public List<FolderDto>? Folders { get; set; }

    [JsonPropertyName("requests")]
    public List<RequestDto>? Requests { get; set; }
}

public class CreateCollectionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_shared")]
    public bool IsShared { get; set; }
}

public class UpdateCollectionDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_shared")]
    public bool? IsShared { get; set; }
}
