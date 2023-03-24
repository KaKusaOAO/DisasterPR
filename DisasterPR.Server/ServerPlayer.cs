using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using ISession = DisasterPR.Sessions.ISession;

namespace DisasterPR.Server;

public class ServerPlayer : ISessionPlayer, ICommandSender
{
    public Guid Id { get; }
    public string Name { get; set; }
    
    public async Task SendMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundChatPacket("系統", "訊息：" + content));
    }

    public async Task SendErrorMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundChatPacket("系統", "錯誤：" + content));
    }

    public ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; } = new();
    public PlayerState State { get; set; }

    public ShuffledPool<WordCard>? CardPool { get; set; }
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer(ServerToPlayerConnection connection)
    {
        Id = Guid.NewGuid();
        Connection = connection;
    }

    public event DisconnectedEventDelegate? Disconnected
    {
        add => Connection.Disconnected += value;
        remove => Connection.Disconnected -= value;
    }
    
    public bool IsConnected => Connection.IsConnected;

    public Task SetCardPackAsync(CardPack pack) => Connection.SendPacketAsync(new ClientboundSetCardPackPacket(pack));
    public Task UpdateSessionOptions(ServerSession session) => 
        Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(session));

    public async Task SendJoinRoomSequenceAsync(ServerSession session)
    {
        await Connection.SendPacketAsync(new ClientboundJoinedRoomPacket(session));
        await Connection.SendPacketAsync(new ClientboundSetCardPackPacket(session.CardPack!));
        await Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(session));
    }

    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundAddPlayerPacket(player));
    
    public Task OnPlayerLeftSessionAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundRemovePlayerPacket(player));

    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundUpdatePlayerStatePacket(player));

    public Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player) =>
        Connection.SendPacketAsync(new ClientboundReplacePlayerPacket(index, player));

    
    public Task KickFromSessionAsync(RoomDisconnectReason reason) => 
        Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(RoomDisconnectReason.Kicked));

    public Task UpdateSessionGameStateAsync(StateOfGame state) => 
        Connection.SendPacketAsync(new ClientboundGameStateChangePacket(state));

    public Task UpdateCurrentPlayerIndexAsync(int index) => 
        Connection.SendPacketAsync(new ClientboundGameCurrentPlayerChangePacket(index));

    public Task UpdatePlayerScoreAsync(ISessionPlayer player, int score) =>
        Connection.SendPacketAsync(new ClientboundUpdatePlayerScorePacket(player, score));

    public Task UpdateWinnerPlayerAsync(Guid id) =>
        Connection.SendPacketAsync(new ClientboundSetWinnerPlayerPacket(id));

    public Task AddChosenWordEntryAsync(Guid id, Guid? playerId, List<int> indices) =>
        Connection.SendPacketAsync(new ClientboundAddChosenWordEntryPacket(id, playerId, indices));

    public Task OnSessionChat(string name, string content) => 
        Connection.SendPacketAsync(new ClientboundChatPacket(name, content));

    public Task UpdatePlayerStateAsync(ISessionPlayer player) =>
        Connection.SendPacketAsync(new ClientboundUpdatePlayerStatePacket(player));

    public Task UpdateCandidateTopicsAsync(int left, int right) =>
        Connection.SendPacketAsync(new ClientboundSetCandidateTopicsPacket(left, right));

    public Task UpdateTimerAsync(int timer) => Connection.SendPacketAsync(new ClientboundUpdateTimerPacket(timer));
    public Task UpdateCurrentTopicAsync(int id) => Connection.SendPacketAsync(new ClientboundSetTopicPacket(id));
    public Task UpdateHoldingWordsAsync(List<int> indices) => Connection.SendPacketAsync(new ClientboundSetWordsPacket(indices));

    public Task RevealChosenWordEntryAsync(Guid id) =>
        Connection.SendPacketAsync(new ClientboundRevealChosenWordEntryPacket(id));

    public Task UpdateFinalWordCardAsync(int index) => Connection.SendPacketAsync(new ClientboundSetFinalPacket(index));

    public Task UpdateRoundCycleAsync(int cycle) =>
        Connection.SendPacketAsync(new ClientboundUpdateRoundCyclePacket(cycle));
}