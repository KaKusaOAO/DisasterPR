using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerStatePacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Id { get; set; }
    public PlayerState State { get; set; }

    public ClientboundUpdatePlayerStatePacket(IPlayer player)
    {
        Id = player.Id;
        State = player.State;
    }

    public ClientboundUpdatePlayerStatePacket(MemoryStream stream)
    {
        Id = stream.ReadGuid();
        State = (PlayerState)stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Id);
        stream.WriteVarInt((int) State);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerState(this);
}