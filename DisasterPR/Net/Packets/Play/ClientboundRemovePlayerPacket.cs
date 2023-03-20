using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRemovePlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }

    public ClientboundRemovePlayerPacket(Guid guid)
    {
        PlayerId = guid;
    }

    public ClientboundRemovePlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
    }

    public ClientboundRemovePlayerPacket(MemoryStream stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(PlayerId);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleRemovePlayerAsync(this);
}