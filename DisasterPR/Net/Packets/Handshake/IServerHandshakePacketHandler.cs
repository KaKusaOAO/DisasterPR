namespace DisasterPR.Net.Packets.Handshake;

public interface IServerHandshakePacketHandler : IServerPacketHandler
{
    public void HandleHello(ServerboundHelloPacket packet);
}