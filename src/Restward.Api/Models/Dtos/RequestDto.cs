using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class RequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("body_content_type")]
    public string? BodyContentType { get; set; }

    [JsonPropertyName("collection_id")]
    public Guid CollectionId { get; set; }

    [JsonPropertyName("folder_id")]
    public Guid? FolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("headers")]
    public List<KeyValuePairDto> Headers { get; set; } = [];

    [JsonPropertyName("parameters")]
    public List<KeyValuePairDto> Parameters { get; set; } = [];
}

public class KeyValuePairDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

public class CreateRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("body_content_type")]
    public string? BodyContentType { get; set; }

    [JsonPropertyName("folder_id")]
    public Guid? FolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }

    [JsonPropertyName("headers")]
    public List<KeyValuePairDto>? Headers { get; set; }

    [JsonPropertyName("parameters")]
    public List<KeyValuePairDto>? Parameters { get; set; }
}

public class UpdateRequestDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("body_content_type")]
    public string? BodyContentType { get; set; }

    [JsonPropertyName("folder_id")]
    public Guid? FolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int? SortOrder { get; set; }

    [JsonPropertyName("headers")]
    public List<KeyValuePairDto>? Headers { get; set; }

    [JsonPropertyName("parameters")]
    public List<KeyValuePairDto>? Parameters { get; set; }
}
