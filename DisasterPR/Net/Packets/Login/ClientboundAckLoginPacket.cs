using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckLoginPacket : IPacket<IClientLoginPacketHandler>
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    
    public ClientboundAckLoginPacket(string name, Guid id)
    {
        Name = name;
        Id = id;
    }
    
    public ClientboundAckLoginPacket(BufferReader stream)
    {
        Name = stream.ReadUtf8String();
        Id = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteUtf8String(Name);
        stream.WriteGuid(Id);
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleAckLogin(this);
}