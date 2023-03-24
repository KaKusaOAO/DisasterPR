namespace DisasterPR.Net.Packets.Play;

public interface IClientPlayPacketHandler : IClientPacketHandler
{
    public void HandleAddPlayer(ClientboundAddPlayerPacket packet);
    public void HandleRemovePlayer(ClientboundRemovePlayerPacket packet);
    public void HandleChat(ClientboundChatPacket packet);
    public void HandleRoomDisconnected(ClientboundRoomDisconnectedPacket packet);
    public void HandleHeartbeat(ClientboundHeartbeatPacket packet);
    public void HandleJoinedRoom(ClientboundJoinedRoomPacket packet);
    public void HandleSetCardPack(ClientboundSetCardPackPacket packet);
    public void HandleSetCandidateTopics(ClientboundSetCandidateTopicsPacket packet);
    public void HandleSetTopic(ClientboundSetTopicPacket packet);
    public void HandleSetWords(ClientboundSetWordsPacket packet);
    public void HandleSetFinal(ClientboundSetFinalPacket packet);
    public void HandleGameStateChange(ClientboundGameStateChangePacket packet);
    public void HandleGameCurrentPlayerChange(ClientboundGameCurrentPlayerChangePacket packet);
    public void HandleAddChosenWordEntry(ClientboundAddChosenWordEntryPacket packet);
    public void HandleUpdateSessionOptions(ClientboundUpdateSessionOptionsPacket packet);
    public void HandleRevealChosenWordEntry(ClientboundRevealChosenWordEntryPacket packet);
    public void HandleUpdatePlayerScore(ClientboundUpdatePlayerScorePacket packet);
    public void HandleSetWinnerPlayer(ClientboundSetWinnerPlayerPacket packet);
    public void HandleUpdateTimer(ClientboundUpdateTimerPacket packet);
    public void HandleUpdateRoundCycle(ClientboundUpdateRoundCyclePacket packet);
    public void HandleUpdatePlayerState(ClientboundUpdatePlayerStatePacket packet);
    public void HandleReplacePlayer(ClientboundReplacePlayerPacket packet);
    public void HandleUpdatePlayerGuid(ClientboundUpdatePlayerGuidPacket packet);
}