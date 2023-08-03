using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Cards;

public class WordCard
{
    public List<CardCategory> Categories { get; set; }
    public PartOfSpeech PartOfSpeech { get; set; }
    public virtual string Label { get; set; }

    public static WordCard Deserialize(CardPack pack, BufferReader reader)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");
        return Deserialize(pack.Categories.ToList(), reader);
    }
    
    public static WordCard Deserialize(CardPack pack, JsonNode? node)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");
        return Deserialize(pack.Categories.ToList(), node);
    }
    
    public static WordCard Deserialize(List<CardCategory> categories, BufferReader reader)
    {
        if (categories == null) throw new ArgumentException($"{nameof(categories)} must present!");

        return new WordCard
        {
            Categories = reader.ReadList(s => CardCategory.DeserializeNoLabel(s, categories)),
            PartOfSpeech = (PartOfSpeech) reader.ReadVarInt(),
            Label = reader.ReadUtf8String()
        };
    }
    
    public static WordCard Deserialize(List<CardCategory> categories, JsonNode? node)
    {
        if (categories == null) throw new ArgumentException($"{nameof(categories)} must present!");

        var obj = (node as JsonObject)!;
        return new WordCard
        {
            Categories = obj["categories"]!.AsArray().Select(s => CardCategory.DeserializeNoLabel(s, categories)).ToList(),
            PartOfSpeech = (PartOfSpeech) obj["pos"]!.GetValue<int>(),
            Label = obj["label"]!.GetValue<string>()
        };
    }

    public void Serialize(BufferWriter writer)
    {
        writer.WriteList(Categories, (s, c) => c.Serialize(s, false));
        writer.WriteVarInt((int) PartOfSpeech);
        writer.WriteUtf8String(Label);
    }

    public JsonObject SerializeToJson()
    {
        return new JsonObject
        {
            ["categories"] = Categories.Select(c => c.SerializeToJson(false)).ToJsonArray(),
            ["pos"] = (int) PartOfSpeech,
            ["label"] = Label
        };
    }
}