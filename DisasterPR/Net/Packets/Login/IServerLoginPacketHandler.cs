namespace DisasterPR.Net.Packets.Login;

public interface IServerLoginPacketHandler : IServerPacketHandler
{
    public Task HandleLoginAsync(ServerboundLoginPacket packet);
}