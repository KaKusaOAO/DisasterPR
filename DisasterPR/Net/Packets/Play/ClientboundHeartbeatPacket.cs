namespace DisasterPR.Net.Packets.Play;

public class ClientboundHeartbeatPacket : IPacket<IClientPlayPacketHandler>
{
    public ClientboundHeartbeatPacket()
    {
        
    }
    
    public ClientboundHeartbeatPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}