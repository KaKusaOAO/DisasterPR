using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateTimerPacket : IPacket<IClientPlayPacketHandler>
{
    public int RemainTime { get; set; }

    public ClientboundUpdateTimerPacket(int time)
    {
        RemainTime = time;
    }

    public ClientboundUpdateTimerPacket(MemoryStream stream)
    {
        RemainTime = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(RemainTime);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateTimer(this);
}