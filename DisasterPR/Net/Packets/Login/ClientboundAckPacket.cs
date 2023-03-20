namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckPacket : IPacket<IClientLoginPacketHandler>
{
    public ClientboundAckPacket()
    {
        
    }
    
    public ClientboundAckPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public Task HandleAsync(IClientLoginPacketHandler handler) => handler.HandleAckAsync(this);
}