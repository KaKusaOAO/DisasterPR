using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundHostRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundHostRoomPacket()
    {
        
    }
    
    public ServerboundHostRoomPacket(BufferReader stream)
    {
        
    } 
    
    public void Write(BufferWriter stream)
    {
        // throw new NotImplementedException();
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHostRoom(this);
}