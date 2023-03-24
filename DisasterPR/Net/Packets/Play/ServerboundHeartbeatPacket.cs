namespace DisasterPR.Net.Packets.Play;

public class ServerboundHeartbeatPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundHeartbeatPacket()
    {
        
    }
    
    public ServerboundHeartbeatPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}