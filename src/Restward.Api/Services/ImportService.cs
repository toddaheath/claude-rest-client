using System.Text.Json;
using Restward.Api.Models.Entities;

namespace Restward.Api.Services;

public class ImportService
{
    public Collection ImportPostmanV2(JsonDocument doc, Guid ownerId)
    {
        var root = doc.RootElement;
        var info = root.GetProperty("info");

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = info.GetProperty("name").GetString() ?? "Imported Collection",
            Description = info.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (root.TryGetProperty("item", out var items))
        {
            ParsePostmanItems(items, collection, null);
        }

        return collection;
    }

    private void ParsePostmanItems(JsonElement items, Collection collection, Folder? parentFolder)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("item", out var subItems))
            {
                var folder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = item.GetProperty("name").GetString() ?? "Folder",
                    CollectionId = collection.Id,
                    ParentFolderId = parentFolder?.Id
                };
                collection.Folders.Add(folder);
                ParsePostmanItems(subItems, collection, folder);
            }
            else if (item.TryGetProperty("request", out var request))
            {
                var requestItem = ParsePostmanRequest(item, request, collection.Id, parentFolder?.Id);
                collection.Requests.Add(requestItem);
            }
        }
    }

    private RequestItem ParsePostmanRequest(JsonElement item, JsonElement request, Guid collectionId, Guid? folderId)
    {
        var method = "GET";
        if (request.TryGetProperty("method", out var m))
            method = m.GetString() ?? "GET";

        var url = "";
        if (request.TryGetProperty("url", out var urlEl))
        {
            if (urlEl.ValueKind == JsonValueKind.String)
                url = urlEl.GetString() ?? "";
            else if (urlEl.TryGetProperty("raw", out var raw))
                url = raw.GetString() ?? "";
        }

        var requestItem = new RequestItem
        {
            Id = Guid.NewGuid(),
            Name = item.GetProperty("name").GetString() ?? "Request",
            Method = method.ToUpperInvariant(),
            Url = url,
            CollectionId = collectionId,
            FolderId = folderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.TryGetProperty("header", out var headers))
        {
            foreach (var h in headers.EnumerateArray())
            {
                requestItem.Headers.Add(new RequestHeader
                {
                    Id = Guid.NewGuid(),
                    Key = h.GetProperty("key").GetString() ?? "",
                    Value = h.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "",
                    Enabled = !h.TryGetProperty("disabled", out var d) || !d.GetBoolean()
                });
            }
        }

        if (request.TryGetProperty("url", out var urlObj) && urlObj.ValueKind == JsonValueKind.Object &&
            urlObj.TryGetProperty("query", out var query))
        {
            foreach (var q in query.EnumerateArray())
            {
                requestItem.Parameters.Add(new RequestParameter
                {
                    Id = Guid.NewGuid(),
                    Key = q.GetProperty("key").GetString() ?? "",
                    Value = q.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "",
                    Enabled = !q.TryGetProperty("disabled", out var d) || !d.GetBoolean()
                });
            }
        }

        if (request.TryGetProperty("body", out var body))
        {
            if (body.TryGetProperty("raw", out var rawBody))
            {
                requestItem.Body = rawBody.GetString();
                requestItem.BodyContentType = "application/json";
                if (body.TryGetProperty("options", out var opts) &&
                    opts.TryGetProperty("raw", out var rawOpts) &&
                    rawOpts.TryGetProperty("language", out var lang))
                {
                    requestItem.BodyContentType = lang.GetString() switch
                    {
                        "xml" => "application/xml",
                        "html" => "text/html",
                        "text" => "text/plain",
                        _ => "application/json"
                    };
                }
            }
        }

        return requestItem;
    }

    public Collection ImportInsomniaV4(JsonDocument doc, Guid ownerId)
    {
        var root = doc.RootElement;
        var resources = root.GetProperty("resources");

        Collection? collection = null;
        var folders = new Dictionary<string, Folder>();
        var requests = new List<(RequestItem Request, string ParentId)>();

        foreach (var resource in resources.EnumerateArray())
        {
            var type = resource.GetProperty("_type").GetString();
            var id = resource.GetProperty("_id").GetString() ?? "";

            switch (type)
            {
                case "workspace":
                    collection = new Collection
                    {
                        Id = Guid.NewGuid(),
                        Name = resource.GetProperty("name").GetString() ?? "Imported",
                        Description = resource.TryGetProperty("description", out var d) ? d.GetString() : null,
                        OwnerId = ownerId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    break;

                case "request_group":
                    var parentId = resource.TryGetProperty("parentId", out var pid) ? pid.GetString() ?? "" : "";
                    var folder = new Folder
                    {
                        Id = Guid.NewGuid(),
                        Name = resource.GetProperty("name").GetString() ?? "Folder"
                    };
                    folders[id] = folder;
                    if (folders.TryGetValue(parentId, out var parentFolder))
                        folder.ParentFolderId = parentFolder.Id;
                    break;

                case "request":
                    var reqParentId = resource.TryGetProperty("parentId", out var rpid) ? rpid.GetString() ?? "" : "";
                    var req = new RequestItem
                    {
                        Id = Guid.NewGuid(),
                        Name = resource.GetProperty("name").GetString() ?? "Request",
                        Method = (resource.TryGetProperty("method", out var rm) ? rm.GetString() : "GET") ?? "GET",
                        Url = (resource.TryGetProperty("url", out var ru) ? ru.GetString() : "") ?? "",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (resource.TryGetProperty("body", out var rb) && rb.ValueKind == JsonValueKind.Object)
                    {
                        if (rb.TryGetProperty("text", out var text))
                            req.Body = text.GetString();
                        if (rb.TryGetProperty("mimeType", out var mime))
                            req.BodyContentType = mime.GetString();
                    }

                    if (resource.TryGetProperty("headers", out var rh))
                    {
                        foreach (var h in rh.EnumerateArray())
                        {
                            req.Headers.Add(new RequestHeader
                            {
                                Id = Guid.NewGuid(),
                                Key = h.GetProperty("name").GetString() ?? "",
                                Value = h.TryGetProperty("value", out var hv) ? hv.GetString() ?? "" : "",
                                Enabled = !h.TryGetProperty("disabled", out var hd) || !hd.GetBoolean()
                            });
                        }
                    }

                    if (resource.TryGetProperty("parameters", out var rp))
                    {
                        foreach (var p in rp.EnumerateArray())
                        {
                            req.Parameters.Add(new RequestParameter
                            {
                                Id = Guid.NewGuid(),
                                Key = p.GetProperty("name").GetString() ?? "",
                                Value = p.TryGetProperty("value", out var pv) ? pv.GetString() ?? "" : "",
                                Enabled = !p.TryGetProperty("disabled", out var pd) || !pd.GetBoolean()
                            });
                        }
                    }

                    requests.Add((req, reqParentId));
                    break;
            }
        }

        collection ??= new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Imported Collection",
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var folder in folders.Values)
        {
            folder.CollectionId = collection.Id;
            collection.Folders.Add(folder);
        }

        foreach (var (req, parentId) in requests)
        {
            req.CollectionId = collection.Id;
            if (folders.TryGetValue(parentId, out var f))
                req.FolderId = f.Id;
            collection.Requests.Add(req);
        }

        return collection;
    }
}
