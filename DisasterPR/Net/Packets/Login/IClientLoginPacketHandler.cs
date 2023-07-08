namespace DisasterPR.Net.Packets.Login;

public interface IClientLoginPacketHandler : IClientSystemChatHandler
{
    public void HandleAckLogin(ClientboundAckLoginPacket packet);
    public void HandleDisconnect(ClientboundDisconnectPacket packet);
}