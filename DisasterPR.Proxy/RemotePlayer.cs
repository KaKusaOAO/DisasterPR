using System.Text;
using System.Text.Json.Nodes;
using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Proxy.Sessions;
using DisasterPR.Sessions;

namespace DisasterPR.Proxy;

public class RemotePlayer : ISessionPlayer
{
    public Guid Id => InternalCreateGuid();

    public string Name { get; set; }
    public ServerSession? Session { get; set; }
    public ShuffledPool<WordCard> CardPool { get; set; }
    public string UpstreamId { get; set; }
    public bool IsRemotePlayer => true;

    private Guid InternalCreateGuid()
    {
        var arr = new byte[16];
        var b = Encoding.UTF8.GetBytes(UpstreamId);
        Array.Copy(b, 0, arr, 0, Math.Min(16, b.Length));
        return new Guid(arr);
    }

    public Task SetCardPackAsync(CardPack pack) => Task.CompletedTask;

    public Task UpdateSessionOptions(ServerSession session) => Task.CompletedTask;

    public Task SendJoinRoomSequenceAsync(ServerSession session, int? selfIndex = null) => Task.CompletedTask;

    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player) => Task.CompletedTask;

    public Task OnPlayerLeftSessionAsync(ISessionPlayer player) => Task.CompletedTask;

    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player) => Task.CompletedTask;

    public Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player) => Task.CompletedTask;

    public Task KickFromSessionAsync(RoomDisconnectReason reason) => Task.CompletedTask;

    public Task KickFromSessionAsync(string reason) => Task.CompletedTask;

    public Task UpdateSessionGameStateAsync(StateOfGame state) => Task.CompletedTask;

    public Task UpdateCurrentPlayerIndexAsync(int index) => Task.CompletedTask;

    public Task UpdatePlayerScoreAsync(ISessionPlayer player, int score) => Task.CompletedTask;

    public Task UpdateWinnerPlayerAsync(Guid id) => Task.CompletedTask;

    public Task AddChosenWordEntryAsync(Guid id, Guid? playerId, List<int> indices) => Task.CompletedTask;

    public Task OnSessionChat(string name, string content) => Task.CompletedTask;

    public Task UpdatePlayerStateAsync(ISessionPlayer player) => Task.CompletedTask;

    public Task UpdateCandidateTopicsAsync(int left, int right) => Task.CompletedTask;

    public Task UpdateTimerAsync(int timer) => Task.CompletedTask;

    public Task UpdateCurrentTopicAsync(int id) => Task.CompletedTask;

    public Task UpdateHoldingWordsAsync(List<HoldingWordCardEntry> entries) => Task.CompletedTask;

    public Task RevealChosenWordEntryAsync(Guid id) => Task.CompletedTask;

    public Task UpdateFinalWordCardAsync(int index) => Task.CompletedTask;

    public Task UpdateRoundCycleAsync(int cycle) => Task.CompletedTask;

    public Task SendToastAsync(string message) => Task.CompletedTask;

    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; }
    public PlayerState State { get; set; }
    public event DisconnectedEventDelegate? Disconnected;
    public bool IsConnected { get; }
}