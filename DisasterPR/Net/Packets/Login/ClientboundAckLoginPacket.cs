using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckLoginPacket : IPacket<IClientLoginPacketHandler>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public ClientboundAckLoginPacket(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public ClientboundAckLoginPacket(BufferReader stream)
    {
        Id = stream.ReadGuid();
        Name = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
        stream.WriteUtf8String(Name);
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleAckLogin(this);
}