using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateRoundCyclePacket : IPacket<IClientPlayPacketHandler>
{
    public int Count { get; set; }

    public ClientboundUpdateRoundCyclePacket(int time)
    {
        Count = time;
    }

    public ClientboundUpdateRoundCyclePacket(BufferReader stream)
    {
        Count = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Count);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateRoundCycle(this);
}