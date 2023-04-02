using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundGameStateChangePacket : IPacket<IClientPlayPacketHandler>
{
    public StateOfGame State { get; set; }
    
    public ClientboundGameStateChangePacket(StateOfGame state)
    {
        State = state;
    }

    public ClientboundGameStateChangePacket(BufferReader stream)
    {
        State = (StateOfGame) stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt((int) State);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleGameStateChange(this);
}