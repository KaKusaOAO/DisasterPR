using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestKickPlayerPacket : IPacket<IServerPlayPacketHandler>
{
    public Guid PlayerId { get; set; }

    public ServerboundRequestKickPlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
    }

    public ServerboundRequestKickPlayerPacket(MemoryStream stream)
    {
        PlayerId = stream.ReadGuid();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(PlayerId);    
    }

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleRequestKickPlayerAsync(this);
}