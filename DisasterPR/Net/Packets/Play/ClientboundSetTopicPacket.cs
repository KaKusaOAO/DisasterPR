using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetTopicPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    
    public ClientboundSetTopicPacket(int id)
    {
        Index = id;
    }

    public ClientboundSetTopicPacket(MemoryStream stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Index);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleSetTopicPacket(this);
}