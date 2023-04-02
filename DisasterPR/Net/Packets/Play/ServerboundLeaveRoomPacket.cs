using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundLeaveRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundLeaveRoomPacket()
    {
        
    }

    public ServerboundLeaveRoomPacket(BufferReader stream)
    {
        
    }
    
    public void Write(BufferWriter stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleLeaveRoom(this);
}