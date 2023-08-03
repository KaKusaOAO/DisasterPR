using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWinnerPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    
    public ClientboundSetWinnerPlayerPacket(Guid id)
    {
        PlayerId = id;
    }

    public ClientboundSetWinnerPlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
    }

    public ClientboundSetWinnerPlayerPacket(JsonObject payload)
    {
        PlayerId = Guid.Parse(payload["id"]!.GetValue<string>());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = PlayerId.ToString();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetWinnerPlayer(this);
}