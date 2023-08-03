using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets;

public enum PacketContentType
{
    Binary,
    Json
}

public class PacketContent
{
    public PacketContentType Type { get; }
    private BufferReader? _reader;
    private JsonObject? _obj;
    
    public PacketContent(BufferReader reader)
    {
        Type = PacketContentType.Binary;
        _reader = reader;
    }
    
    public PacketContent(JsonObject obj)
    {
        Type = PacketContentType.Json;
        _obj = obj;
    }
    
    public static implicit operator BufferReader(PacketContent content) => content.GetAsBufferReader();

    public BufferReader GetAsBufferReader()
    {
        if (Type != PacketContentType.Binary)
            throw new Exception("Content is not binary");
        return _reader!;
    }
    
    public JsonObject GetAsJsonObject()
    {
        if (Type != PacketContentType.Json)
            throw new Exception("Content is not JSON");
        return _obj!;
    }
}