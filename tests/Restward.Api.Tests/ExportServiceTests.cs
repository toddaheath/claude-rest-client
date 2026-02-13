using Restward.Api.Models.Entities;
using Restward.Api.Services;

namespace Restward.Api.Tests;

public class ExportServiceTests
{
    private readonly ExportService _exportService = new();

    private Collection CreateTestCollection()
    {
        var collectionId = Guid.NewGuid();
        var folderId = Guid.NewGuid();

        return new Collection
        {
            Id = collectionId,
            Name = "Test API",
            Description = "Test collection",
            OwnerId = Guid.NewGuid(),
            Folders =
            [
                new Folder
                {
                    Id = folderId,
                    Name = "Auth",
                    CollectionId = collectionId,
                    SortOrder = 0
                }
            ],
            Requests =
            [
                new RequestItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Get Users",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    CollectionId = collectionId,
                    SortOrder = 0,
                    Headers =
                    [
                        new RequestHeader { Id = Guid.NewGuid(), Key = "Accept", Value = "application/json", Enabled = true }
                    ],
                    Parameters =
                    [
                        new RequestParameter { Id = Guid.NewGuid(), Key = "page", Value = "1", Enabled = true }
                    ]
                },
                new RequestItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Login",
                    Method = "POST",
                    Url = "https://api.example.com/login",
                    Body = "{\"username\": \"test\"}",
                    BodyContentType = "application/json",
                    CollectionId = collectionId,
                    FolderId = folderId,
                    SortOrder = 0,
                    Headers = [],
                    Parameters = []
                }
            ]
        };
    }

    [Fact]
    public void ExportPostmanV2_ProducesValidStructure()
    {
        var collection = CreateTestCollection();
        var result = _exportService.ExportPostmanV2(collection);

        Assert.Equal("Test API", result["info"]!["name"]!.ToString());
        Assert.NotNull(result["item"]);

        var items = result["item"]!.AsArray();
        Assert.Equal(2, items.Count); // 1 folder + 1 root request
    }

    [Fact]
    public void ExportInsomniaV4_ProducesValidStructure()
    {
        var collection = CreateTestCollection();
        var result = _exportService.ExportInsomniaV4(collection);

        Assert.Equal("export", result["_type"]!.ToString());
        Assert.Equal(4, result["__export_format"]!.GetValue<int>());

        var resources = result["resources"]!.AsArray();
        Assert.True(resources.Count >= 3); // workspace + folder + requests
        Assert.Equal("workspace", resources[0]!["_type"]!.ToString());
    }

    [Fact]
    public void ExportPostmanV2_IncludesRequestBody()
    {
        var collection = CreateTestCollection();
        var result = _exportService.ExportPostmanV2(collection);

        var items = result["item"]!.AsArray();
        // The folder item should contain a Login request with body
        var folderItem = items[0]!;
        var folderRequests = folderItem["item"]!.AsArray();
        Assert.Single(folderRequests);

        var loginRequest = folderRequests[0]!["request"]!;
        Assert.NotNull(loginRequest["body"]);
        Assert.Equal("raw", loginRequest["body"]!["mode"]!.ToString());
    }
}
