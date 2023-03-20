using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; }

    public ClientboundAddPlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
        PlayerName = player.Name;
    }

    public ClientboundAddPlayerPacket(MemoryStream stream)
    {
        PlayerId = stream.ReadGuid();
        PlayerName = stream.ReadUtf8String();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(PlayerId);
        stream.WriteUtf8String(PlayerName);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleAddPlayerAsync(this);
}