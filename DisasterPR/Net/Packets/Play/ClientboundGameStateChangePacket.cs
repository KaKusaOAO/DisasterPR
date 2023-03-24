using DisasterPR.Extensions;
using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundGameStateChangePacket : IPacket<IClientPlayPacketHandler>
{
    public StateOfGame State { get; set; }
    
    public ClientboundGameStateChangePacket(StateOfGame state)
    {
        State = state;
    }

    public ClientboundGameStateChangePacket(MemoryStream stream)
    {
        State = (StateOfGame) stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt((int) State);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleGameStateChange(this);
}