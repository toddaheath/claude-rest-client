using System.Text.Json.Serialization;

namespace Restward.Api.Models.Dtos;

public class FolderDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("collection_id")]
    public Guid CollectionId { get; set; }

    [JsonPropertyName("parent_folder_id")]
    public Guid? ParentFolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }

    [JsonPropertyName("sub_folders")]
    public List<FolderDto>? SubFolders { get; set; }

    [JsonPropertyName("requests")]
    public List<RequestDto>? Requests { get; set; }
}

public class CreateFolderDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parent_folder_id")]
    public Guid? ParentFolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; set; }
}

public class UpdateFolderDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("parent_folder_id")]
    public Guid? ParentFolderId { get; set; }

    [JsonPropertyName("sort_order")]
    public int? SortOrder { get; set; }
}
