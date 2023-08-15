using System.Text.Json.Nodes;
using Mochi.IO;
using Mochi.Utils;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestUpdateNamePacket : IPacket<IServerPlayPacketHandler>
{
    public string? Name { get; set; }

    public ServerboundRequestUpdateNamePacket(string? name = null)
    {
        Name = name;
    }
    
    public ServerboundRequestUpdateNamePacket(BufferReader stream)
    {
        Name = stream.ReadOptional(s => s.ReadUtf8String()).OrElse((string?) null);
    }
    
    public ServerboundRequestUpdateNamePacket(JsonObject payload)
    {
        Name = payload["name"]?.GetValue<string>();
    }

    public void Write(BufferWriter writer)
    {
        writer.WriteOptional(Name!, (s, v) => s.WriteUtf8String(v!));
    }

    public void Write(JsonObject obj)
    {
        obj["name"] = Name;
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestUpdateName(this);
}