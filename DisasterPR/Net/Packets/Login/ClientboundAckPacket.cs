using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckPacket : IPacket<IClientLoginPacketHandler>
{
    public Guid Id { get; set; }
    
    public ClientboundAckPacket(Guid id)
    {
        Id = id;
    }
    
    public ClientboundAckPacket(MemoryStream stream)
    {
        Id = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Id);
    }

    public Task HandleAsync(IClientLoginPacketHandler handler) => handler.HandleAckAsync(this);
}