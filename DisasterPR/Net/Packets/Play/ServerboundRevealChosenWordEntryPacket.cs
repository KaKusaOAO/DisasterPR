using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRevealChosenWordEntryPacket : IPacket<IServerPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ServerboundRevealChosenWordEntryPacket(Guid guid)
    {
        Guid = guid;
    }

    public ServerboundRevealChosenWordEntryPacket(BufferReader stream)
    {
        Guid = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRevealChosenWordEntry(this);
}