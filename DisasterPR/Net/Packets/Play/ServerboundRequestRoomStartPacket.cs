using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestRoomStartPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundRequestRoomStartPacket()
    {
        
    }

    public ServerboundRequestRoomStartPacket(BufferReader stream)
    {
        
    }
    
    public void Write(BufferWriter stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestRoomStart(this);
}