using DisasterPR.Extensions;

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

    public static CardCategory Deserialize(Stream stream)
    {
        var isBuiltin = stream.ReadBool();
        if (isBuiltin)
        {
            var id = stream.ReadVarInt();
            return _map[id];
        }

        var guid = stream.ReadGuid();
        var label = stream.ReadUtf8String();
        return new CardCategory(guid, label);
    }
    
    public static CardCategory DeserializeNoLabel(Stream stream, CardPack pack) => 
        DeserializeNoLabel(stream, pack.Categories.ToList());

    public static CardCategory DeserializeNoLabel(Stream stream, List<CardCategory> categories)
    {
        var isBuiltin = stream.ReadBool();
        if (isBuiltin)
        {
            var id = stream.ReadVarInt();
            return _map[id];
        }

        var guid = stream.ReadGuid();
        return categories.First(c => c.Guid == guid);
    }

    public void Serialize(Stream stream, bool writeLabel = true)
    {
        stream.WriteBool(IsBuiltin);
        if (IsBuiltin)
        {
            stream.WriteVarInt(Id);
            return;
        }
        
        stream.WriteGuid(Guid);
        
        if (!writeLabel) return;
        stream.WriteUtf8String(Label);
    }
}