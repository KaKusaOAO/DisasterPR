namespace DisasterPR.Net.Packets.Play;

public interface IServerPlayPacketHandler : IServerPacketHandler
{
    public Task HandleChatAsync(ServerboundChatPacket packet);
    public Task HandleHostRoomAsync(ServerboundHostRoomPacket packet);
    public Task HandleJoinRoomAsync(ServerboundJoinRoomPacket packet);
    public Task HandleHeartbeatAsync(ServerboundHeartbeatPacket packet);
}