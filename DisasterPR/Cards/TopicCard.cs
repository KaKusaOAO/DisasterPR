using System.Reflection.Emit;
using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Cards;

public class TopicCard
{
    public List<CardCategory> Categories { get; set; }
    public List<string> Texts { get; set; } = new();

    public int AnswerCount => Texts.Count - 1;
    
    public static TopicCard Deserialize(CardPack pack, BufferReader reader)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");
        return Deserialize(pack.Categories.ToList(), reader);
    }

    public static TopicCard Deserialize(CardPack pack, JsonNode? node)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");
        return Deserialize(pack.Categories.ToList(), node);
    }
    
    public static TopicCard Deserialize(List<CardCategory> categories, JsonNode? node)
    {
        if (categories == null) throw new ArgumentException($"{nameof(categories)} must present!");
        var obj = (node as JsonObject)!;
        
        return new TopicCard()
        {
            Categories = obj["categories"]!.AsArray().Select(s => CardCategory.DeserializeNoLabel(s, categories))
                .ToList(),
            Texts = obj["texts"]!.AsArray().Select(s => s!.GetValue<string>()).ToList()
        };
    }
    
    public static TopicCard Deserialize(List<CardCategory> categories, BufferReader reader)
    {
        if (categories == null) throw new ArgumentException($"{nameof(categories)} must present!");

        return new TopicCard
        {
            Categories = reader.ReadList(s => CardCategory.DeserializeNoLabel(s, categories)),
            Texts = reader.ReadList(s => s.ReadUtf8String())
        };
    }

    public void Serialize(BufferWriter writer)
    {
        writer.WriteList(Categories, (s, c) => c.Serialize(s, false));
        writer.WriteList(Texts, (s, str) => s.WriteUtf8String(str));
    }

    public JsonObject SerializeToJson()
    {
        return new JsonObject
        {
            ["categories"] = Categories.Select(c => c.SerializeToJson(false)).ToJsonArray(),
            ["texts"] = Texts.Select(s => JsonValue.Create(s)!).ToJsonArray()
        };
    }
}