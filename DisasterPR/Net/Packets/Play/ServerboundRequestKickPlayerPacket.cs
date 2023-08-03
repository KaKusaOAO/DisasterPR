using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestKickPlayerPacket : IPacket<IServerPlayPacketHandler>
{
    public Guid PlayerId { get; set; }

    public ServerboundRequestKickPlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
    }

    public ServerboundRequestKickPlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);    
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = PlayerId.ToString();
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestKickPlayer(this);
}