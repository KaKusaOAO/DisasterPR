using DisasterPR.Net.Packets.Handshake;

namespace DisasterPR.Net.Packets;

public interface IServerHandshakePacketHandler : IServerPacketHandler
{
    public void HandleHello(ServerboundHelloPacket packet);
}