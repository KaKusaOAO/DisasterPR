using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRevealChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ClientboundRevealChosenWordEntryPacket(Guid guid)
    {
        Guid = guid;
    }

    public ClientboundRevealChosenWordEntryPacket(BufferReader stream)
    {
        Guid = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRevealChosenWordEntry(this);
}