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

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleHeartbeatAsync(this);
}