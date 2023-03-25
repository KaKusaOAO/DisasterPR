using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Extensions;
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

        Name = player.Name;
        OriginalName = player is AIPlayer ai ? ai.OriginalName : Name;
        
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
    
    public AIPlayer()
    {
        Id = Guid.NewGuid();
        Name = "AI-#" + Random.Shared.Next(1000);
    }

    public Task SetCardPackAsync(CardPack pack) => Task.CompletedTask;
    public Task UpdateSessionOptions(ServerSession session) => Task.CompletedTask;
    public Task SendJoinRoomSequenceAsync(ServerSession session, int? selfIndex = null) => Task.CompletedTask;
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
        
        // Process AI step
        var stateOpt = Session?.GameState.CurrentState;
        if (!stateOpt.HasValue) return;

        var state = stateOpt.Value;
        var context = Session!.ServerGameState;
        
        if (state == StateOfGame.ChoosingTopic && context.CurrentPlayer == this)
        {
            var side = Random.Shared.Next(2) > 0 ? HorizontalSide.Left : HorizontalSide.Right;
            await context.ChooseTopicAsync(side);
        }
        
        if (state == StateOfGame.ChoosingWord && context.CurrentPlayer != this)
        {
            if (context.CurrentChosenWords.All(c => c.Player != this))
            {
                var count = context.CurrentTopic.AnswerCount;
                var list = HoldingCards.Shuffled().Take(count).ToList();
                await context.ChooseWordAsync(this, list);
            }
        }

        if (state == StateOfGame.ChoosingFinal && context.CurrentPlayer == this)
        {
            if (Session.ServerGameState.HasChosenFinal) return;
            
            var unrevealed = context.CurrentChosenWords.Find(w => !w.IsRevealed);
            if (unrevealed != null)
            {
                await context.RevealChosenWordEntryAsync(unrevealed.Id);
                return;
            }

            var index = Random.Shared.Next(context.CurrentChosenWords.Count);
            await context.RevealChosenWordEntryAsync(context.CurrentChosenWords[index].Id);
            await context.ChooseFinalAsync(this, index);
        }
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