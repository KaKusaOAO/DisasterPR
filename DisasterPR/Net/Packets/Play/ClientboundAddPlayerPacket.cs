using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; }

    public ClientboundAddPlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
        PlayerName = player.Name;
    }

    public ClientboundAddPlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
        PlayerName = stream.ReadUtf8String();
    }
    
    public ClientboundAddPlayerPacket(JsonObject payload)
    {
        PlayerId = Guid.Parse(payload["id"]!.GetValue<string>());
        PlayerName = payload["name"]!.GetValue<string>();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);
        stream.WriteUtf8String(PlayerName);
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = PlayerId.ToString();
        obj["name"] = PlayerName;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleAddPlayer(this);
}