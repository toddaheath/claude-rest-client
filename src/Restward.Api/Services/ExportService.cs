using System.Text.Json;
using System.Text.Json.Nodes;
using Restward.Api.Models.Entities;

namespace Restward.Api.Services;

public class ExportService
{
    public JsonObject ExportPostmanV2(Collection collection)
    {
        var root = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["name"] = collection.Name,
                ["description"] = collection.Description ?? "",
                ["schema"] = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            ["item"] = BuildPostmanItems(collection)
        };

        return root;
    }

    private JsonArray BuildPostmanItems(Collection collection)
    {
        var items = new JsonArray();

        var rootFolders = collection.Folders
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.SortOrder);

        foreach (var folder in rootFolders)
        {
            items.Add(BuildPostmanFolder(folder, collection));
        }

        var rootRequests = collection.Requests
            .Where(r => r.FolderId == null)
            .OrderBy(r => r.SortOrder);

        foreach (var request in rootRequests)
        {
            items.Add(BuildPostmanRequest(request));
        }

        return items;
    }

    private JsonObject BuildPostmanFolder(Folder folder, Collection collection)
    {
        var item = new JsonObject
        {
            ["name"] = folder.Name,
            ["item"] = new JsonArray()
        };

        var subFolders = collection.Folders
            .Where(f => f.ParentFolderId == folder.Id)
            .OrderBy(f => f.SortOrder);

        foreach (var sub in subFolders)
        {
            ((JsonArray)item["item"]!).Add(BuildPostmanFolder(sub, collection));
        }

        var requests = collection.Requests
            .Where(r => r.FolderId == folder.Id)
            .OrderBy(r => r.SortOrder);

        foreach (var req in requests)
        {
            ((JsonArray)item["item"]!).Add(BuildPostmanRequest(req));
        }

        return item;
    }

    private JsonObject BuildPostmanRequest(RequestItem request)
    {
        var headers = new JsonArray();
        foreach (var h in request.Headers)
        {
            headers.Add(new JsonObject
            {
                ["key"] = h.Key,
                ["value"] = h.Value,
                ["disabled"] = !h.Enabled
            });
        }

        var query = new JsonArray();
        foreach (var p in request.Parameters)
        {
            query.Add(new JsonObject
            {
                ["key"] = p.Key,
                ["value"] = p.Value,
                ["disabled"] = !p.Enabled
            });
        }

        var urlObj = new JsonObject
        {
            ["raw"] = request.Url,
            ["query"] = query
        };

        var requestObj = new JsonObject
        {
            ["method"] = request.Method,
            ["header"] = headers,
            ["url"] = urlObj
        };

        if (request.Body is not null)
        {
            var language = request.BodyContentType switch
            {
                "application/xml" => "xml",
                "text/html" => "html",
                "text/plain" => "text",
                _ => "json"
            };

            requestObj["body"] = new JsonObject
            {
                ["mode"] = "raw",
                ["raw"] = request.Body,
                ["options"] = new JsonObject
                {
                    ["raw"] = new JsonObject { ["language"] = language }
                }
            };
        }

        return new JsonObject
        {
            ["name"] = request.Name,
            ["request"] = requestObj
        };
    }

    public JsonObject ExportInsomniaV4(Collection collection)
    {
        var resources = new JsonArray();
        var workspaceId = $"wrk_{collection.Id:N}";

        resources.Add(new JsonObject
        {
            ["_id"] = workspaceId,
            ["_type"] = "workspace",
            ["name"] = collection.Name,
            ["description"] = collection.Description ?? ""
        });

        foreach (var folder in collection.Folders.OrderBy(f => f.SortOrder))
        {
            var parentId = folder.ParentFolderId.HasValue
                ? $"fld_{collection.Folders.First(f => f.Id == folder.ParentFolderId).Id:N}"
                : workspaceId;

            resources.Add(new JsonObject
            {
                ["_id"] = $"fld_{folder.Id:N}",
                ["_type"] = "request_group",
                ["name"] = folder.Name,
                ["parentId"] = parentId
            });
        }

        foreach (var req in collection.Requests.OrderBy(r => r.SortOrder))
        {
            var parentId = req.FolderId.HasValue
                ? $"fld_{req.FolderId:N}"
                : workspaceId;

            var headersArray = new JsonArray();
            foreach (var h in req.Headers)
            {
                headersArray.Add(new JsonObject
                {
                    ["name"] = h.Key,
                    ["value"] = h.Value,
                    ["disabled"] = !h.Enabled
                });
            }

            var paramsArray = new JsonArray();
            foreach (var p in req.Parameters)
            {
                paramsArray.Add(new JsonObject
                {
                    ["name"] = p.Key,
                    ["value"] = p.Value,
                    ["disabled"] = !p.Enabled
                });
            }

            var reqObj = new JsonObject
            {
                ["_id"] = $"req_{req.Id:N}",
                ["_type"] = "request",
                ["name"] = req.Name,
                ["method"] = req.Method,
                ["url"] = req.Url,
                ["headers"] = headersArray,
                ["parameters"] = paramsArray,
                ["parentId"] = parentId
            };

            if (req.Body is not null)
            {
                reqObj["body"] = new JsonObject
                {
                    ["mimeType"] = req.BodyContentType ?? "application/json",
                    ["text"] = req.Body
                };
            }

            resources.Add(reqObj);
        }

        return new JsonObject
        {
            ["_type"] = "export",
            ["__export_format"] = 4,
            ["resources"] = resources
        };
    }
}
