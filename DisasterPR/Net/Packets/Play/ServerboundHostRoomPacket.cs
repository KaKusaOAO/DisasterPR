namespace DisasterPR.Net.Packets.Play;

public class ServerboundHostRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundHostRoomPacket()
    {
        
    }
    
    public ServerboundHostRoomPacket(MemoryStream stream)
    {
        
    } 
    
    public void Write(MemoryStream stream)
    {
        // throw new NotImplementedException();
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHostRoom(this);
}