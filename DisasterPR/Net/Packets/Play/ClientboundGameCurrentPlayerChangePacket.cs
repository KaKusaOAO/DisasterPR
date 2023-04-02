using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundGameCurrentPlayerChangePacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    
    public ClientboundGameCurrentPlayerChangePacket(int index)
    {
        Index = index;
    }

    public ClientboundGameCurrentPlayerChangePacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleGameCurrentPlayerChange(this);
}