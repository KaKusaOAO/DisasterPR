namespace DisasterPR.Net.Packets.Play;

public interface IServerPlayPacketHandler : IServerPacketHandler
{
    public void HandleChat(ServerboundChatPacket packet);
    public void HandleHostRoom(ServerboundHostRoomPacket packet);
    public void HandleJoinRoom(ServerboundJoinRoomPacket packet);
    public void HandleHeartbeat(ServerboundHeartbeatPacket packet);
    public void HandleChooseTopic(ServerboundChooseTopicPacket packet);
    public void HandleRequestRoomStart(ServerboundRequestRoomStartPacket packet);
    public void HandleChooseWord(ServerboundChooseWordPacket packet);
    public void HandleChooseFinal(ServerboundChooseFinalPacket packet);
    public void HandleRevealChosenWordEntry(ServerboundRevealChosenWordEntryPacket packet);
    public void HandleUpdateSessionOptions(ServerboundUpdateSessionOptionsPacket packet);
    public void HandleLeaveRoom(ServerboundLeaveRoomPacket packet);
    public void HandleRequestKickPlayer(ServerboundRequestKickPlayerPacket packet);
    public void HandleUpdatePlayerState(ServerboundUpdatePlayerStatePacket packet);
    public void HandleUpdateLockedWord(ServerboundUpdateLockedWordPacket packet);
    public void HandleRequestShuffleWords(ServerboundRequestShuffleWordsPacket packet);
    public void HandleRequestRandomName(ServerboundRequestRandomNamePacket packet);
    public void HandleRequestUpdateName(ServerboundRequestUpdateNamePacket packet);
}