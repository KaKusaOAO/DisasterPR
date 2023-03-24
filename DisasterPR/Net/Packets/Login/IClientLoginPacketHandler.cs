namespace DisasterPR.Net.Packets.Login;

public interface IClientLoginPacketHandler : IClientPacketHandler
{
    public void HandleAck(ClientboundAckPacket packet);
    public void HandleDisconnect(ClientboundDisconnectPacket packet);
}