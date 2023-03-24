using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerGuidPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ClientboundUpdatePlayerGuidPacket(Guid guid)
    {
        Guid = guid;
    }

    public ClientboundUpdatePlayerGuidPacket(MemoryStream stream)
    {
        Guid = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerGuid(this);
}