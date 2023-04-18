namespace DisasterPR.Net.Packets.Login;

public interface IClientLoginPacketHandler : IClientPacketHandler
{
    public void HandleAckLogin(ClientboundAckLoginPacket packet);
    public void HandleDisconnect(ClientboundDisconnectPacket packet);
}