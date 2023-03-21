namespace DisasterPR.Net.Packets.Play;

public class ServerboundLeaveRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundLeaveRoomPacket()
    {
        
    }

    public ServerboundLeaveRoomPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public Task HandleAsync(IServerPlayPacketHandler handler)
    {
        throw new NotImplementedException();
    }
}