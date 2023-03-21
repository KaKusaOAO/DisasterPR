namespace DisasterPR.Net.Packets.Play;

public interface IClientPlayPacketHandler : IClientPacketHandler
{
    public Task HandleAddPlayerAsync(ClientboundAddPlayerPacket packet);
    public Task HandleRemovePlayerAsync(ClientboundRemovePlayerPacket packet);
    public Task HandleChatAsync(ClientboundChatPacket packet);
    public Task HandleRoomDisconnectedAsync(ClientboundRoomDisconnectedPacket packet);
    public Task HandleHeartbeatAsync(ClientboundHeartbeatPacket packet);
    public Task HandleJoinedRoomAsync(ClientboundJoinedRoomPacket packet);
    public Task HandleSetCardPackAsync(ClientboundSetCardPackPacket packet);
    public Task HandleSetCandidateTopicsPacket(ClientboundSetCandidateTopicsPacket packet);
    public Task HandleSetTopicPacket(ClientboundSetTopicPacket packet);
    public Task HandleSetWordsPacket(ClientboundSetWordsPacket packet);
    public Task HandleSetFinalPacket(ClientboundSetFinalPacket packet);
    public Task HandleGameStateChangeAsync(ClientboundGameStateChangePacket packet);
    public Task HandleGameCurrentPlayerChangeAsync(ClientboundGameCurrentPlayerChangePacket packet);
    public Task HandleAddChosenWordEntryAsync(ClientboundAddChosenWordEntryPacket packet);
    public Task HandleUpdateSessionOptionsPacket(ClientboundUpdateSessionOptionsPacket packet);
    public Task HandleRevealChosenWordEntryAsync(ClientboundRevealChosenWordEntryPacket packet);
    public Task HandleUpdatePlayerScoreAsync(ClientboundUpdatePlayerScorePacket packet);
    public Task HandleSetWinnerPlayerPacket(ClientboundSetWinnerPlayerPacket packet);
}