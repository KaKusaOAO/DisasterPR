using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRevealChosenWordEntryPacket : IPacket<IServerPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ServerboundRevealChosenWordEntryPacket(Guid guid)
    {
        Guid = guid;
    }

    public ServerboundRevealChosenWordEntryPacket(MemoryStream stream)
    {
        Guid = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Guid);
    }

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleRevealChosenWordEntryAsync(this);
}