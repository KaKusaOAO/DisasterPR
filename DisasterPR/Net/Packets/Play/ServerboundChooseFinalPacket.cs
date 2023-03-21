using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChooseFinalPacket : IPacket<IServerPlayPacketHandler>
{
    public int Index { get; set; }

    public ServerboundChooseFinalPacket(int index)
    {
        Index = index;
    }

    public ServerboundChooseFinalPacket(MemoryStream stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Index);
    }

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleChooseFinalAsync(this);
}