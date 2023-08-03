using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRemovePlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }

    public ClientboundRemovePlayerPacket(Guid guid)
    {
        PlayerId = guid;
    }

    public ClientboundRemovePlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
    }

    public ClientboundRemovePlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public ClientboundRemovePlayerPacket(JsonObject payload)
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

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRemovePlayer(this);
}