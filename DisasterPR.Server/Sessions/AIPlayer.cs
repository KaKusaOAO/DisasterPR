using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Sessions;

namespace DisasterPR.Server.Sessions;

public class AIPlayer : ISessionPlayer
{
    public Guid Id { get; }
    public string Name { get; }
    public string OriginalName { get; }
    public ServerSession? Session { get; set; }
    public ShuffledPool<WordCard> CardPool { get; set; }

    public AIPlayer(ISessionPlayer player)
    {
        Id = player.Id;
        OriginalName = Name = player.Name;
        if (player is ServerPlayer)
        {
            Name += "的幽靈";
        }

        CardPool = player.CardPool;
        State = player.State;
        Score = player.Score;
        Session = player.Session;
        HoldingCards.AddRange(player.HoldingCards);
    }

    public Task SetCardPackAsync(CardPack pack) => Task.CompletedTask;
    public Task UpdateSessionOptions(ServerSession session) => Task.CompletedTask;
    public Task SendJoinRoomSequenceAsync(ServerSession session) => Task.CompletedTask;
    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player) => Task.CompletedTask;
    public Task OnPlayerLeftSessionAsync(ISessionPlayer player) => Task.CompletedTask;
    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player) => Task.CompletedTask;
    public Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player) => Task.CompletedTask;

    public Task KickFromSessionAsync(RoomDisconnectReason reason)
    {
        Disconnected?.Invoke(new DisconnectedEventArgs
        {
            Reason = PlayerKickReason.Disconnected
        });
        return Task.CompletedTask;
    }

    public Task UpdateSessionGameStateAsync(StateOfGame state) => Task.CompletedTask;
    public Task UpdateCurrentPlayerIndexAsync(int index) => Task.CompletedTask;

    public Task UpdatePlayerScoreAsync(ISessionPlayer player, int score) => Task.CompletedTask;

    public Task UpdateWinnerPlayerAsync(Guid id) => Task.CompletedTask;

    public Task AddChosenWordEntryAsync(Guid id, Guid? playerId, List<int> indices) => Task.CompletedTask;
    public Task OnSessionChat(string name, string content) => Task.CompletedTask;

    public Task UpdatePlayerStateAsync(ISessionPlayer player) => Task.CompletedTask;

    public Task UpdateCandidateTopicsAsync(int left, int right) => Task.CompletedTask;

    public async Task UpdateTimerAsync(int timer)
    {
        await Task.Yield();
    }

    public Task UpdateCurrentTopicAsync(int id) => Task.CompletedTask;

    public Task UpdateHoldingWordsAsync(List<int> indices) => Task.CompletedTask;

    public Task RevealChosenWordEntryAsync(Guid id) => Task.CompletedTask;

    public Task UpdateFinalWordCardAsync(int index) => Task.CompletedTask;

    public Task UpdateRoundCycleAsync(int cycle) => Task.CompletedTask;

    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; } = new();
    public PlayerState State { get; set; }
    public event DisconnectedEventDelegate? Disconnected;
    public bool IsConnected => true;
}