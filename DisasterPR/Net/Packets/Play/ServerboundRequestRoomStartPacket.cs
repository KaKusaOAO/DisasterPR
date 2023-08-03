using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestRoomStartPacket : IPacketNoContent<IServerPlayPacketHandler>
{
    public ServerboundRequestRoomStartPacket()
    {
        
    }

    public ServerboundRequestRoomStartPacket(BufferReader stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestRoomStart(this);
}