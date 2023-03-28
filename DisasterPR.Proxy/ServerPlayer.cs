using System.Text.Json.Nodes;
using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Proxy.Commands.Senders;
using DisasterPR.Proxy.Sessions;
using DisasterPR.Sessions;
using ISession = DisasterPR.Sessions.ISession;
using LogLevel = KaLib.Utils.LogLevel;

namespace DisasterPR.Proxy;

public class ServerPlayer : ISessionPlayer, ICommandSender
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string UpstreamId => Math.Abs(Id.GetHashCode()).ToString();
    public bool IsRemotePlayer => false;

    public async Task SendMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundSystemChatPacket(content));
    }

    public async Task SendErrorMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundSystemChatPacket(content, LogLevel.Error));
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

    public async Task SendJoinRoomSequenceAsync(ServerSession session, int? selfIndex = null)
    {
        await Connection.SendPacketAsync(new ClientboundJoinedRoomPacket(session, selfIndex));
        await Connection.SendPacketAsync(new ClientboundSetCardPackPacket(session.CardPack!));
        await Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(session));
    }

    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundAddPlayerPacket(player));
    
    public Task OnPlayerLeftSessionAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundRemovePlayerPacket(player));

    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player) => 
        Connection.SendPacketAsync(new ClientboundUpdatePlayerStatePacket(player));

    public async Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player)
    {
        await Connection.SendPacketAsync(new ClientboundReplacePlayerPacket(index, player));
        await Connection.SendPacketAsync(new ClientboundUpdatePlayerScorePacket(player, player.Score));
    }
    
    public Task KickFromSessionAsync(RoomDisconnectReason reason) => 
        Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(RoomDisconnectReason.Kicked));
    
    public Task KickFromSessionAsync(string reason) => 
        Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(reason));

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
    public Task UpdateHoldingWordsAsync(List<HoldingWordCardEntry> entries)
    {
        var data = entries.Select(h => new ClientboundSetWordsPacket.Entry
        {
            IsLocked = h.IsLocked,
            Index = Session!.CardPack!.GetWordIndex(h.Card)
        }).ToList();
        
        return Connection.SendPacketAsync(new ClientboundSetWordsPacket(data));
    }

    public Task RevealChosenWordEntryAsync(Guid id) =>
        Connection.SendPacketAsync(new ClientboundRevealChosenWordEntryPacket(id));

    public Task UpdateFinalWordCardAsync(int index) => Connection.SendPacketAsync(new ClientboundSetFinalPacket(index));

    public Task UpdateRoundCycleAsync(int cycle) =>
        Connection.SendPacketAsync(new ClientboundUpdateRoundCyclePacket(cycle));

    public Task SendToastAsync(string message) =>
        Connection.SendPacketAsync(new ClientboundSystemChatPacket(message));
}