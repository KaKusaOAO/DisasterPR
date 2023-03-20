using DisasterPR.Net.Packets.Handshake;

namespace DisasterPR.Net.Packets;

public interface IServerHandshakePacketHandler : IServerPacketHandler
{
    public Task HandleHelloAsync(ServerboundHelloPacket packet);
}