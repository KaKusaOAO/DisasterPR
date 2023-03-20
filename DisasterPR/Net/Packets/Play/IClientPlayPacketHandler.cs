namespace DisasterPR.Net.Packets.Play;

public interface IClientPlayPacketHandler : IClientPacketHandler
{
    public Task HandleAddPlayerAsync(ClientboundAddPlayerPacket packet);
    public Task HandleRemovePlayerAsync(ClientboundRemovePlayerPacket packet);
    public Task HandleSessionStartAsync(ClientboundSessionStartPacket packet);
    public Task HandleChatAsync(ClientboundChatPacket packet);
    public Task HandleRoomDisconnectedAsync(ClientboundRoomDisconnectedPacket packet);
    public Task HandleHeartbeatAsync(ClientboundHeartbeatPacket packet);
    public Task HandleJoinedRoomAsync(ClientboundJoinedRoomPacket packet);
}