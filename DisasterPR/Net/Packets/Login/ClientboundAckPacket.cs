using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckPacket : IPacket<IClientLoginPacketHandler>
{
    public Guid Id { get; set; }
    
    public ClientboundAckPacket(Guid id)
    {
        Id = id;
    }
    
    public ClientboundAckPacket(BufferReader stream)
    {
        Id = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleAck(this);
}