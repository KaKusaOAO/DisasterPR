using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWinnerPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    
    public ClientboundSetWinnerPlayerPacket(Guid id)
    {
        PlayerId = id;
    }

    public ClientboundSetWinnerPlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetWinnerPlayer(this);
}