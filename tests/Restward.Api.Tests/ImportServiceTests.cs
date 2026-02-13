using System.Text.Json;
using Restward.Api.Services;

namespace Restward.Api.Tests;

public class ImportServiceTests
{
    private readonly ImportService _importService = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    [Fact]
    public void ImportPostmanV2_ParsesBasicCollection()
    {
        var json = """
        {
          "info": { "name": "Test API", "description": "A test collection" },
          "item": [
            {
              "name": "Get Users",
              "request": {
                "method": "GET",
                "url": { "raw": "https://api.example.com/users", "query": [{ "key": "page", "value": "1" }] },
                "header": [{ "key": "Accept", "value": "application/json" }]
              }
            }
          ]
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var collection = _importService.ImportPostmanV2(doc, _ownerId);

        Assert.Equal("Test API", collection.Name);
        Assert.Equal("A test collection", collection.Description);
        Assert.Equal(_ownerId, collection.OwnerId);
        Assert.Single(collection.Requests);

        var request = collection.Requests[0];
        Assert.Equal("Get Users", request.Name);
        Assert.Equal("GET", request.Method);
        Assert.Equal("https://api.example.com/users", request.Url);
        Assert.Single(request.Headers);
        Assert.Equal("Accept", request.Headers[0].Key);
        Assert.Single(request.Parameters);
        Assert.Equal("page", request.Parameters[0].Key);
    }

    [Fact]
    public void ImportPostmanV2_ParsesNestedFolders()
    {
        var json = """
        {
          "info": { "name": "Nested" },
          "item": [
            {
              "name": "Auth",
              "item": [
                {
                  "name": "Login",
                  "request": { "method": "POST", "url": "https://api.example.com/login" }
                }
              ]
            }
          ]
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var collection = _importService.ImportPostmanV2(doc, _ownerId);

        Assert.Single(collection.Folders);
        Assert.Equal("Auth", collection.Folders[0].Name);
        Assert.Single(collection.Requests);
        Assert.Equal("Login", collection.Requests[0].Name);
    }

    [Fact]
    public void ImportPostmanV2_ParsesRequestBody()
    {
        var json = """
        {
          "info": { "name": "Body Test" },
          "item": [
            {
              "name": "Create User",
              "request": {
                "method": "POST",
                "url": "https://api.example.com/users",
                "body": {
                  "mode": "raw",
                  "raw": "{\"name\": \"test\"}",
                  "options": { "raw": { "language": "json" } }
                }
              }
            }
          ]
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var collection = _importService.ImportPostmanV2(doc, _ownerId);

        var request = collection.Requests[0];
        Assert.Equal("{\"name\": \"test\"}", request.Body);
        Assert.Equal("application/json", request.BodyContentType);
    }

    [Fact]
    public void ImportInsomniaV4_ParsesBasicCollection()
    {
        var json = """
        {
          "resources": [
            { "_id": "wrk_1", "_type": "workspace", "name": "My API" },
            {
              "_id": "req_1", "_type": "request", "name": "Get Users",
              "method": "GET", "url": "https://api.example.com/users",
              "headers": [{ "name": "Accept", "value": "application/json" }],
              "parameters": [{ "name": "limit", "value": "10" }],
              "parentId": "wrk_1"
            }
          ]
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var collection = _importService.ImportInsomniaV4(doc, _ownerId);

        Assert.Equal("My API", collection.Name);
        Assert.Single(collection.Requests);

        var request = collection.Requests[0];
        Assert.Equal("Get Users", request.Name);
        Assert.Equal("GET", request.Method);
        Assert.Single(request.Headers);
        Assert.Single(request.Parameters);
    }

    [Fact]
    public void ImportInsomniaV4_ParsesFolders()
    {
        var json = """
        {
          "resources": [
            { "_id": "wrk_1", "_type": "workspace", "name": "API" },
            { "_id": "fld_1", "_type": "request_group", "name": "Auth", "parentId": "wrk_1" },
            {
              "_id": "req_1", "_type": "request", "name": "Login",
              "method": "POST", "url": "https://api.example.com/login",
              "headers": [], "parameters": [],
              "parentId": "fld_1"
            }
          ]
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var collection = _importService.ImportInsomniaV4(doc, _ownerId);

        Assert.Single(collection.Folders);
        Assert.Equal("Auth", collection.Folders.First().Name);
        Assert.Single(collection.Requests);
        Assert.NotNull(collection.Requests[0].FolderId);
    }
}
