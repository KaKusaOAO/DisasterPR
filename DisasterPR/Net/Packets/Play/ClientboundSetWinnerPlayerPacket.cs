using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWinnerPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    
    public ClientboundSetWinnerPlayerPacket(Guid id)
    {
        PlayerId = id;
    }

    public ClientboundSetWinnerPlayerPacket(MemoryStream stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(PlayerId);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleSetWinnerPlayerPacket(this);
}