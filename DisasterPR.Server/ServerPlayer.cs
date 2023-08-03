using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Platforms;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using Mochi.Utils;
using ISession = DisasterPR.Sessions.ISession;
using LogLevel = Mochi.Utils.LogLevel;

namespace DisasterPR.Server;

public class ServerPlayer : ISessionPlayer, ICommandSender
{
    private IPlatformData _platformData;
    public PlayerPlatform LoginType { get; set; }

    public IPlatformData PlatformData
    {
        get => _platformData;
        set
        {
            if (_platformData != null!) _platformData.Updated -= PlatformDataOnUpdated;
            _platformData = value;
            if (value != null!) value.Updated += PlatformDataOnUpdated;
        }
    }

    private void PlatformDataOnUpdated()
    {
        var players = new List<ServerPlayer>();
        if (Session == null) players.Add(this);
        else players.AddRange(Session.Players.OfType<ServerPlayer>());
        Logger.Info($"Platform data updated for {Name}. Sending updates for {players.Count} players");

        foreach (var player in players)
        {
            _ = player.Connection.SendPacketAsync(new ClientboundUpdatePlayerDataPacket(this));
        }
    }

    public string Identifier => PlatformData.Identifier;
    public byte[]? AvatarData => PlatformData.AvatarData;
    public Guid Id { get; set; }
    public string Name { get; set; }

    public T GetPlatformDataAs<T>() where T : IPlatformData => (T) PlatformData;
    
    public async Task SendMessageAsync(string content)
    {
        await SendToastAsync(content);
    }

    public async Task SendErrorMessageAsync(string content)
    {
        await SendToastAsync(content, LogLevel.Error);
    }

    public ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; } = new();
    public PlayerState State { get; set; }

    public ShuffledPool<WordCard>? CardPool { get; set; }
    public bool IsManuallyShuffled { get; set; }
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer(ServerToPlayerConnection connection)
    {
        Id = Guid.NewGuid();
        Connection = connection;
        Name = "<unknown>";
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

    public Task SendToastAsync(string message, LogLevel level = LogLevel.Info) =>
        Connection.SendPacketAsync(new ClientboundSystemChatPacket(message, level));
}