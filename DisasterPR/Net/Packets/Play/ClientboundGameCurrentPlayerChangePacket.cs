using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

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

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleGameCurrentPlayerChange(this);
}