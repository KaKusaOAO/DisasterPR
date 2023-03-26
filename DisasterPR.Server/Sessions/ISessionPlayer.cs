using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Sessions;
using ISession = DisasterPR.Sessions.ISession;

namespace DisasterPR.Server.Sessions;

public interface ISessionPlayer : IPlayer
{
    public event DisconnectedEventDelegate Disconnected;
    
    public bool IsConnected { get; }
    
    public new ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    public ShuffledPool<WordCard> CardPool { get; set; }

    public Task SetCardPackAsync(CardPack pack);
    public Task UpdateSessionOptions(ServerSession session);

    public Task SendJoinRoomSequenceAsync(ServerSession session, int? selfIndex = null);
    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player);
    public Task OnPlayerLeftSessionAsync(ISessionPlayer player);
    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player);
    public Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player);
    public Task KickFromSessionAsync(RoomDisconnectReason reason);
    public Task KickFromSessionAsync(string reason);
    public Task UpdateSessionGameStateAsync(StateOfGame state);
    public Task UpdateCurrentPlayerIndexAsync(int index);
    public Task UpdatePlayerScoreAsync(ISessionPlayer player, int score);
    public Task UpdateWinnerPlayerAsync(Guid id);
    public Task AddChosenWordEntryAsync(Guid id, Guid? playerId, List<int> indices);
    public Task OnSessionChat(string name, string content);
    public Task UpdatePlayerStateAsync(ISessionPlayer player);
    public Task UpdateCandidateTopicsAsync(int left, int right);
    public Task UpdateTimerAsync(int timer);
    public Task UpdateCurrentTopicAsync(int id);
    public Task UpdateHoldingWordsAsync(List<HoldingWordCardEntry> entries);
    public Task RevealChosenWordEntryAsync(Guid id);
    public Task UpdateFinalWordCardAsync(int index);
    public Task UpdateRoundCycleAsync(int cycle);
    public Task SendToastAsync(string message);
}