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

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleHostRoomAsync(this);
}