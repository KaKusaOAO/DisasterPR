using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRevealChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ClientboundRevealChosenWordEntryPacket(Guid guid)
    {
        Guid = guid;
    }

    public ClientboundRevealChosenWordEntryPacket(MemoryStream stream)
    {
        Guid = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRevealChosenWordEntry(this);
}