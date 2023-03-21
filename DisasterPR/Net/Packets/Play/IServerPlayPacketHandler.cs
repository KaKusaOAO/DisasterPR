namespace DisasterPR.Net.Packets.Play;

public interface IServerPlayPacketHandler : IServerPacketHandler
{
    public Task HandleChatAsync(ServerboundChatPacket packet);
    public Task HandleHostRoomAsync(ServerboundHostRoomPacket packet);
    public Task HandleJoinRoomAsync(ServerboundJoinRoomPacket packet);
    public Task HandleHeartbeatAsync(ServerboundHeartbeatPacket packet);
    public Task HandleChooseTopicAsync(ServerboundChooseTopicPacket packet);
    public Task HandleRequestRoomStartAsync(ServerboundRequestRoomStartPacket packet);
    public Task HandleChooseWordAsync(ServerboundChooseWordPacket packet);
    public Task HandleChooseFinalAsync(ServerboundChooseFinalPacket packet);
    public Task HandleRevealChosenWordEntryAsync(ServerboundRevealChosenWordEntryPacket packet);
    public Task HandleUpdateSessionOptionsPacket(ServerboundUpdateSessionOptionsPacket packet);
}