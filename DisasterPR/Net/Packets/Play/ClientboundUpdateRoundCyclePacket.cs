using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateRoundCyclePacket : IPacket<IClientPlayPacketHandler>
{
    public int Count { get; set; }

    public ClientboundUpdateRoundCyclePacket(int time)
    {
        Count = time;
    }

    public ClientboundUpdateRoundCyclePacket(MemoryStream stream)
    {
        Count = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Count);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleUpdateRoundCycleAsync(this);
}