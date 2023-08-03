using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerGuidPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid OldGuid { get; set; }
    public Guid NewGuid { get; set; }

    public ClientboundUpdatePlayerGuidPacket(Guid old, Guid current)
    {
        OldGuid = old;
        NewGuid = current;
    }

    public ClientboundUpdatePlayerGuidPacket(BufferReader stream)
    {
        OldGuid = stream.ReadGuid();
        NewGuid = stream.ReadGuid();
    }

    public ClientboundUpdatePlayerGuidPacket(JsonObject payload)
    {
        OldGuid = Guid.Parse(payload["old"]!.GetValue<string>());
        NewGuid = Guid.Parse(payload["current"]!.GetValue<string>());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(OldGuid);
        stream.WriteGuid(NewGuid);
    }

    public void Write(JsonObject obj)
    {
        obj["old"] = OldGuid.ToString();
        obj["current"] = OldGuid.ToString();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerGuid(this);
}