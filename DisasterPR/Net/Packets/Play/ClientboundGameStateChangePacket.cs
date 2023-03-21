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

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleGameStateChangeAsync(this);
}

public class ClientboundGameCurrentPlayerChangePacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    
    public ClientboundGameCurrentPlayerChangePacket(int index)
    {
        Index = index;
    }

    public ClientboundGameCurrentPlayerChangePacket(MemoryStream stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Index);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleGameCurrentPlayerChangeAsync(this);
}