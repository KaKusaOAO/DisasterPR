using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Firebase.Database.Query;
using KaLib.Utils;

namespace DisasterPR.Proxy.Net.Firebase;

public class FieldObserver : IDisposable
{
    public ChildQuery Query { get; }
    private JsonObject? _cached = new();
    private bool _disposed;

    public event Action<string, JsonObject?>? Updated;

    public FieldObserver(ChildQuery query)
    {
        Query = query;
    }

    public FieldObserver StartObserving(Action<string, JsonObject?>? observer = null)
    {
        Updated += observer;
        _ = RunEventLoopAsync();
        return this;
    }

    private async Task RunEventLoopAsync()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        
        while (!_disposed)
        {
            var uri = new Uri(await Query.BuildUrlAsync());
            var stream = await http.GetStreamAsync(uri);
            using var reader = new StreamReader(stream);

            var type = "";

            while (!_disposed)
            {
                var line = "";
                while (!_disposed)
                {
                    line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    break;
                }

                if (_disposed) return;

                var arr = line.Split(':', 2).Select(s => s.Trim()).ToArray();
                if (arr[0] == "event")
                {
                    type = arr[1];
                }

                if (arr[0] == "data")
                {
                    if (type == "cancel" || type == "auth-revoke") break;
                    if (type == "keep-alive") continue;

                    if (type == "put" || type == "patch")
                    {
                        try
                        {
                            Logger.Info($"{type}: {arr[1]}");
                            var json = JsonSerializer.Deserialize<JsonObject>(arr[1])!;
                            var path = json["path"]!.GetValue<string>();
                            var data = json["data"]!;

                            if (path == "/")
                            {
                                var obj = data?.AsObject();

                                if (type == "patch")
                                {
                                    _cached ??= new JsonObject();
                                    foreach (var (key, value) in obj!)
                                    {
                                        var val = JsonNode.Parse(value?.ToJsonString() ?? "null");
                                        _cached.AddOrSet(key, val);
                                    }
                                }
                                else
                                {
                                    _cached = obj;
                                }

                                if (obj == null)
                                {
                                    Updated?.Invoke("", _cached);
                                }
                                else
                                {
                                    foreach (var (key, _) in obj!)
                                    {
                                        Updated?.Invoke(key, _cached);
                                    }
                                }
                            }
                            else
                            {
                                var curr = _cached ??= new JsonObject();
                                var itemName = null as string;

                                var pathArr = path[1..].Split('/');
                                for (var i = 0; i < pathArr.Length; i++)
                                {
                                    var dir = pathArr[i];
                                    if (i + 1 != pathArr.Length)
                                    {
                                        var obj = new JsonObject();
                                        if (curr.ContainsKey(dir))
                                        {
                                            curr = curr[dir]!.AsObject();
                                        }
                                        else
                                        {
                                            curr.AddOrSet(dir, obj);
                                            curr = obj;
                                        }

                                        continue;
                                    }

                                    itemName = dir;
                                }

                                var val = JsonNode.Parse(data?.ToJsonString() ?? "null");
                                if (curr.ContainsKey(itemName!))
                                {
                                    if (type == "put")
                                    {
                                        if (val == null && curr.ContainsKey(itemName))
                                        {
                                            curr.Remove(itemName);
                                        }
                                        else
                                        {
                                            curr.AddOrSet(itemName, val);
                                        }
                                    }
                                    else
                                    {
                                        if (val is JsonObject valp)
                                        {
                                            var obj = curr[itemName].AsObject();
                                            foreach (var (k, _) in valp)
                                            {
                                                var v = JsonNode.Parse(valp[k]?.ToJsonString() ?? "null");
                                                if (v == null && obj.ContainsKey(k))
                                                {
                                                    obj.Remove(k);
                                                }
                                                else
                                                {
                                                    obj.AddOrSet(k, v);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (val != null)
                                    {
                                        curr.AddOrSet(itemName, val);
                                    }
                                }
                                
                                Updated?.Invoke(pathArr[0], _cached);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex.ToString());
                        }
                        
                        continue;
                    }
                    
                    Logger.Warn($"event type: {type}");
                }
            }

            if (_disposed) return;
            await Task.Delay(2000);
            
        }
    }

    public void Dispose()
    {
        Query.Dispose();
        _disposed = true;
    }
}