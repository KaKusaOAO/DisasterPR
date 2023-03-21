using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetFinalPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }

    public ClientboundSetFinalPacket(int index)
    {
        Index = index;
    }

    public ClientboundSetFinalPacket(MemoryStream stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Index);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleSetFinalPacket(this);
}