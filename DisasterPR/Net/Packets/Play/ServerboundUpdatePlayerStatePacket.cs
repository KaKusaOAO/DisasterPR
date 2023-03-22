using DisasterPR.Extensions;
using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundUpdatePlayerStatePacket : IPacket<IServerPlayPacketHandler>
{
    public PlayerState State { get; set; }

    public ServerboundUpdatePlayerStatePacket(IPlayer player)
    {
        State = player.State;
    }

    public ServerboundUpdatePlayerStatePacket(MemoryStream stream)
    {
        State = (PlayerState)stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt((int) State);
    }

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleUpdatePlayerStateAsync(this);
}