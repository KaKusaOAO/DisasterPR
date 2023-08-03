using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundUpdatePlayerStatePacket : IPacket<IServerPlayPacketHandler>
{
    public PlayerState State { get; set; }

    public ServerboundUpdatePlayerStatePacket(IPlayer player)
    {
        State = player.State;
    }

    public ServerboundUpdatePlayerStatePacket(BufferReader stream)
    {
        State = (PlayerState)stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt((int) State);
    }

    public void Write(JsonObject obj)
    {
        obj["state"] = (int) State;
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleUpdatePlayerState(this);
}