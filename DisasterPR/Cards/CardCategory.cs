using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Cards;

public class CardCategory
{
    private static readonly Dictionary<int, CardCategory> _map = new();

    public static readonly CardCategory Generic = new("普通");
    public static readonly CardCategory R18 = new("R18");
    public static readonly CardCategory Hell = new("地獄梗");
    public static readonly CardCategory ReligionOrPolitics = new("宗教政治");
    public static readonly CardCategory Acg = new("動畫/VTuber/遊戲");
    public static readonly CardCategory SocialMedia = new("電影/社交媒體");
    public static readonly CardCategory Unpopular = new("冷門梗");

    public int Id { get; } = -1;
    public Guid Guid { get; }
    public bool IsBuiltin { get; }
    public string Label { get; }
    
    private CardCategory(string label)
    {
        var id = _map.Count;
        Id = id;

        var arr = new byte[16];
        var conv = BitConverter.GetBytes(id);
        Array.Copy(conv, arr, conv.Length);
        Guid = new Guid(arr);
        
        IsBuiltin = true;
        Label = label;
        _map.Add(id, this);
    }

    public CardCategory(Guid guid, string label)
    {
        Guid = guid;
        Label = label;
    }

    public static List<CardCategory> Builtins => _map.Values.ToList();

    public static CardCategory Deserialize(BufferReader reader)
    {
        var isBuiltin = reader.ReadBool();
        if (isBuiltin)
        {
            var id = reader.ReadVarInt();
            return _map[id];
        }

        var guid = reader.ReadGuid();
        var label = reader.ReadUtf8String();
        return new CardCategory(guid, label);
    }

    public static CardCategory Deserialize(JsonNode? node)
    {
        var obj = (node as JsonObject)!;
        if (obj.TryGetPropertyValue("builtin", out var value))
        {
            var id = value!.GetValue<int>();
            return _map[id];
        }
        
        var guid = Guid.Parse(obj["id"]!.GetValue<string>());
        var label = obj["label"]!.GetValue<string>();
        return new CardCategory(guid, label);
    }
    
    public static CardCategory DeserializeNoLabel(BufferReader reader, CardPack pack) => 
        DeserializeNoLabel(reader, pack.Categories.ToList());

    public static CardCategory DeserializeNoLabel(BufferReader reader, List<CardCategory> categories)
    {
        var isBuiltin = reader.ReadBool();
        if (isBuiltin)
        {
            var id = reader.ReadVarInt();
            return _map[id];
        }

        var guid = reader.ReadGuid();
        return categories.First(c => c.Guid == guid);
    }

    public static CardCategory DeserializeNoLabel(JsonNode? node, List<CardCategory> categories)
    {
        var obj = (node as JsonObject)!;
        if (obj.TryGetPropertyValue("builtin", out var value))
        {
            var id = value!.GetValue<int>();
            return _map[id];
        }
        
        var guid = Guid.Parse(obj["id"]!.GetValue<string>());
        return categories.First(c => c.Guid == guid);
    }

    public void Serialize(BufferWriter writer, bool writeLabel = true)
    {
        writer.WriteBool(IsBuiltin);
        if (IsBuiltin)
        {
            writer.WriteVarInt(Id);
            return;
        }
        
        writer.WriteGuid(Guid);
        
        if (!writeLabel) return;
        writer.WriteUtf8String(Label);
    }

    public JsonObject SerializeToJson(bool writeLabel = true)
    {
        var obj = new JsonObject();
        if (IsBuiltin)
        {
            obj["builtin"] = Id;
            return obj;
        }

        obj["guid"] = Guid.ToString();
        if (writeLabel) obj["label"] = Label;
        return obj;
    }
}