using System.Text.Json;
using System.Text.Json.Nodes;
using KaLib.Utils;

namespace DisasterPR.Cards.Providers;

public class UpstreamFormatPackProvider : IPackProvider
{
    public Uri Uri { get; set; }
    
    public UpstreamFormatPackProvider(Uri uri)
    {
        Uri = uri;
    }
    
    public virtual async Task<CardPackBuilder> MakeBuilderAsync()
    {
        var http = new HttpClient();
        var request = await http.GetStreamAsync(Uri);
        var data = (await JsonSerializer.DeserializeAsync<JsonObject>(request))!;
        var builder = CardPackBuilder.Create().AddBuiltinCategories();
        
        var pack = data["CardPack"]!.AsObject();
        var topicOneFill = pack["一填空題目"]!.AsObject();
        var topicTwoFill = pack["兩填空題目"]!.AsObject();
        var words = pack["答案"]!.AsObject();

        string FromFiller(string str) => str == "[]" ? "" : str;

        string GetValueAsString(JsonNode node)
        {
            try
            {
                return node.GetValue<string>();
            }
            catch (InvalidOperationException)
            {
                return node.ToJsonString();
            }
        }

        foreach (var (key, value) in topicOneFill)
        {
            if (value is not JsonObject) continue;
            
            var entry = value!.AsObject();
            var a = FromFiller(GetValueAsString(entry["內容1"]!));
            var b = FromFiller(GetValueAsString(entry["內容2"]!));
            var categories = entry["標籤"]!.GetValue<int>().ToString()
                .ToCharArray()
                .Select(n =>
                {
                    var i = n - '0' - 1;
                    return CardCategory.Builtins[i];
                });
            builder.AddTopic(categories, new[] {a, b});
        }
        
        foreach (var (key, value) in topicTwoFill)
        {
            if (value is not JsonObject) continue;
            
            var entry = value!.AsObject();
            var a = FromFiller(GetValueAsString(entry["內容1"]!));
            var b = FromFiller(GetValueAsString(entry["內容2"]!));
            var c = FromFiller(GetValueAsString(entry["內容3"]!));
            var categories = entry["標籤"]!.GetValue<int>().ToString()
                .ToCharArray()
                .Select(n =>
                {
                    var i = n - '0' - 1;
                    return CardCategory.Builtins[i];
                });
            builder.AddTopic(categories, new[] {a, b, c});
        }

        foreach (var (key, value) in words)
        {
            if (value is not JsonObject) continue;
            
            var entry = value!.AsObject();
            var label = FromFiller(GetValueAsString(entry["內容"]!));
            
            var pos = entry["詞性"]!.GetValue<string>() switch
            {
                "動詞" => PartOfSpeech.Verb,
                "名詞" => PartOfSpeech.Noun,
                "形容詞" => PartOfSpeech.Adjective,
                _ => PartOfSpeech.Unknown
            };
                
            var categories = entry["標籤"]!.GetValue<int>().ToString()
                .ToCharArray()
                .Select(n =>
                {
                    var i = n - '0' - 1;
                    return CardCategory.Builtins[i];
                });
            builder.AddWord(categories, pos, label);
        }

        return builder;
    }
}