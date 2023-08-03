using System.Text.Json.Nodes;

namespace DisasterPR.Extensions;

public static class JsonExtension
{
    public static JsonArray ToJsonArray<T>(this IEnumerable<T> nodes) where T : JsonNode => 
        new(nodes.Select(x => (JsonNode) x).ToArray());

    public static IEnumerable<T> AsArray<T>(this JsonNode arr, Func<JsonNode?, T> convert) =>
        arr.AsArray().Select(convert);
}