namespace DisasterPR.Net.Packets.Login;

public interface IServerLoginPacketHandler : IServerPacketHandler
{
    public void HandleLogin(ServerboundLoginPacket packet);
}