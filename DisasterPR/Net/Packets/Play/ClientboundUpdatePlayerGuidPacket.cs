using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerGuidPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ClientboundUpdatePlayerGuidPacket(Guid guid)
    {
        Guid = guid;
    }

    public ClientboundUpdatePlayerGuidPacket(BufferReader stream)
    {
        Guid = stream.ReadGuid();
    }

    public ClientboundUpdatePlayerGuidPacket(JsonObject payload)
    {
        Guid = Guid.Parse(payload["id"]!.GetValue<string>());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = Guid.ToString();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerGuid(this);
}