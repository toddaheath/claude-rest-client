using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restward.Api.Models.Entities;

[Table("folders")]
public class Folder
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("collection_id")]
    public Guid CollectionId { get; set; }

    [ForeignKey(nameof(CollectionId))]
    public Collection Collection { get; set; } = null!;

    [Column("parent_folder_id")]
    public Guid? ParentFolderId { get; set; }

    [ForeignKey(nameof(ParentFolderId))]
    public Folder? ParentFolder { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    public List<Folder> SubFolders { get; set; } = [];
    public List<RequestItem> Requests { get; set; } = [];
}
