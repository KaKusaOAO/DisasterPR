namespace DisasterPR.Net.Packets.Login;

public interface IClientLoginPacketHandler : IClientPacketHandler
{
    public Task HandleAckAsync(ClientboundAckPacket packet);
    public Task HandleDisconnectAsync(ClientboundDisconnectPacket packet);
}