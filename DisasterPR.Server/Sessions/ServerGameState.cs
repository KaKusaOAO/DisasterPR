using System.Collections.Concurrent;
using System.Diagnostics;
using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;
using ISession = DisasterPR.Sessions.ISession;
using SessionOptions = DisasterPR.Sessions.SessionOptions;

namespace DisasterPR.Server.Sessions;

public class ServerGameState : IGameState
{
    public ServerSession Session { get; }
    ISession IGameState.Session => Session;

    public StateOfGame CurrentState { get; set; }
    public int CurrentPlayerIndex { get; set; }

    public ServerPlayer CurrentPlayer => Session.Players[CurrentPlayerIndex];
    IPlayer IGameState.CurrentPlayer => CurrentPlayer;

    public ServerPlayer? WinnerPlayer { get; set; }
    IPlayer? IGameState.WinnerPlayer => WinnerPlayer;

    public TopicCard CurrentTopic { get; set; }

    public List<ServerChosenWordEntry> CurrentChosenWords { get; } = new();

    List<IChosenWordEntry> IGameState.CurrentChosenWords =>
        CurrentChosenWords.Select(n => (IChosenWordEntry) n).ToList();

    public (TopicCard Left, TopicCard Right)? CandidateTopics { get; private set; }
    public int RoundCycle { get; set; } = 1;
    
    private ShuffledPool<TopicCard> _topics;

    private CancellationTokenSource _cts = new();

    public SessionOptions Options => Session.Options;

    private List<ServerChosenWordEntry> _chosenWords = new();

    private ConcurrentQueue<Func<Task>> _actions = new();

    private Thread _thread;

    public ServerGameState(ServerSession session)
    {
        Session = session;

        _thread = new Thread(async () => await InternalEventLoopAsync())
        {
            Name = $"Session #{session.RoomId}",
            IsBackground = true
        };
        _thread.Start();
    }

    private async Task InternalEventLoopAsync()
    {
        while (Session.IsValid)
        {
            SpinWait.SpinUntil(() => !_actions.IsEmpty);

            while (!_actions.IsEmpty)
            {
                if (!Session.IsValid) return;
                if (!_actions.TryDequeue(out var action)) continue;

                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Caught exception in event loop!");
                    Logger.Warn(ex.ToString());
                }
            }
        }
    }

    private void ShuffleTopicsAndWords()
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => ShuffleTopicsAndWords());
            return;
        }

        _topics = new ShuffledPool<TopicCard>(Session.CardPack.Topics);

        foreach (var player in Session.Players)
        {
            player.CardPool = new ShuffledPool<WordCard>(Session.CardPack.Words);
        }
    }

    private async Task ChangeStateAndUpdateAsync(StateOfGame state)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangeStateAndUpdateAsync(state));
            return;
        }

        CurrentState = state;
        _cts.Cancel();
        Logger.Verbose($"Current state changed to {state}");
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundGameStateChangePacket(state))));
    }

    private async Task ChangeCurrentPlayerIndexAndUpdateAsync(int index)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangeCurrentPlayerIndexAndUpdateAsync(index));
            return;
        }

        CurrentPlayerIndex = index;
        Logger.Verbose($"Current player is now {CurrentPlayer.Name} ({CurrentPlayer.Id})");
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundGameCurrentPlayerChangePacket(index))));
    }

    private async Task ChangePlayerScoreAndUpdateAsync(ServerPlayer player, int score)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangePlayerScoreAndUpdateAsync(player, score));
            return;
        }

        player.Score = score;
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundUpdatePlayerScorePacket(player, score))));
    }

    private async Task ChangeWinnerPlayerAndUpdateAsync(ServerPlayer player)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangeWinnerPlayerAndUpdateAsync(player));
            return;
        }

        WinnerPlayer = player;
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundSetWinnerPlayerPacket(player.Id))));
    }

    public async Task StartAsync()
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(StartAsync);
            return;
        }
        
        ShuffleTopicsAndWords();
        await ChangeStateAndUpdateAsync(StateOfGame.Started);
        await ChangeCurrentPlayerIndexAndUpdateAsync(0);

        foreach (var p in Session.Players)
        {
            await ChangePlayerScoreAndUpdateAsync(p, 0);
        }

        await ChangeRoundCycleCountAndUpdateAsync(1);
        await StartRoundAsync();
    }

    public async Task StartRoundAsync()
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(StartRoundAsync);
            return;
        }

        CurrentChosenWords.Clear();

        (TopicCard Left, TopicCard Right) topics = (_topics.Next(), _topics.Next());

        var pack = Session.CardPack;
        var left = pack.GetTopicIndex(topics.Left);
        var right = pack.GetTopicIndex(topics.Right);
        CandidateTopics = topics;

        await CurrentPlayer.Connection.SendPacketAsync(new ClientboundSetCandidateTopicsPacket(left, right));
        await ChangeStateAndUpdateAsync(StateOfGame.ChoosingTopic);
        _ = ChooseOtherRandomTopicAsync();
    }

    private async Task ChooseOtherRandomTopicAsync()
    {
        // Called when timed out
        var time = Options.CountdownTimeSet.TopicChooseTime;
        if (!await WaitForTimerAsync(time)) return;

        var topic = _topics.Next();
        await SetTopicAsync(topic);
    }

    public async Task ChooseTopicAsync(HorizontalSide side)
    {
        if (!CandidateTopics.HasValue) return;
        _cts.Cancel();
        var topic = side == HorizontalSide.Left ? CandidateTopics.Value.Left : CandidateTopics.Value.Right;
        await SetTopicAsync(topic);
    }

    public async Task ChooseWordAsync(ServerPlayer player, List<WordCard> cards)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChooseWordAsync(player, cards));
            return;
        }

        if (cards.Any(card => !player.HoldingCards.Contains(card)))
        {
            Logger.Warn($"Player {player.Name} is choosing non-existing card!");
            return;
        }

        var entry = new ServerChosenWordEntry(this, player, cards);
        CurrentChosenWords.Add(entry);

        var pack = Session.CardPack;
        var words = cards.Select(w => pack.GetWordIndex(w)).ToList();
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundAddChosenWordEntryPacket(entry.Id, entry.Player?.Id, words))));

        if (Session.Players
            .Where(p => p != CurrentPlayer)
            .All(p => CurrentChosenWords.Find(w => w.Player == p) != null))
        {
            Logger.Info($"Transitioning into final...");
            _cts.Cancel();
            await StartFinalAsync();
        }
        else
        {
            var count = Session.Players
                .Where(p => p != CurrentPlayer)
                .Count(p => CurrentChosenWords.Find(w => w.Player == p) != null);
            Logger.Info($"Now has {count}/{Session.Players.Count - 1} chosen words, still need more.");
        }
    }

    private async Task StartFinalAsync()
    {
        await ChangeStateAndUpdateAsync(StateOfGame.ChoosingFinal);
        _ = SkipFinalAsync();
    }

    private async Task SkipFinalAsync()
    {
        var time = Options.CountdownTimeSet.FinalChooseTime;
        if (!await WaitForTimerAsync(time)) return;
        await PrepareNextRoundAsync();
    }

    private async Task<bool> WaitForTimerAsync(int time)
    {
        _cts = new CancellationTokenSource();

        void SendTimerUpdate()
        {
            foreach (var player in Session.Players)
            {
                _ = player.Connection.SendPacketAsync(new ClientboundUpdateTimerPacket(time));
            }
        }

        while (time > 0)
        {
            SendTimerUpdate();
            await Task.Delay(1000, _cts.Token);
            if (_cts.IsCancellationRequested) return false;
            time--;
        }
        
        SendTimerUpdate();
        return true;
    }

    private async Task ChooseEmptyWordsForPlayersAsync()
    {
        // Called when timed out
        var time = Options.CountdownTimeSet.AnswerChooseTime;
        if (!await WaitForTimerAsync(time)) return;

        async Task RunChooseEmptyWordAsync()
        {
            var players = Session.Players
                .Where(p => p != CurrentPlayer)
                .Where(p => CurrentChosenWords.All(w => w.Player != p));
            
            foreach (var _ in players)
            {
                var entry = new ServerChosenWordEntry(this, null, new List<WordCard>());
                CurrentChosenWords.Add(entry);

                await Task.WhenAll(Session.Players.Select(p =>
                    p.Connection.SendPacketAsync(
                        new ClientboundAddChosenWordEntryPacket(entry.Id, entry.Player?.Id, new List<int>()))));
            }

            await StartFinalAsync();
        }
        
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(RunChooseEmptyWordAsync);
            return;
        }

        await RunChooseEmptyWordAsync();
    }

    public async Task SetTopicAsync(TopicCard topic)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await SetTopicAsync(topic));
            return;
        }
        
        if (CurrentState != StateOfGame.ChoosingTopic)
        {
            throw new InvalidOperationException("Attempted to choose topic when it's not the time to do it");
        }

        CurrentTopic = topic;

        async Task SendTopicAndWordsAsync(ServerPlayer p)
        {
            var conn = p.Connection;
            var pack = Session.CardPack;
            var id = pack.GetTopicIndex(CurrentTopic);
            var words = new List<WordCard>();

            for (var i = 0; i < 11; i++)
            {
                words.Add(p.CardPool!.Next());
            }

            p.HoldingCards.Clear();
            p.HoldingCards.AddRange(words);

            var indices = words.Select(w => pack.GetWordIndex(w)).ToList();
            await conn.SendPacketAsync(new ClientboundSetTopicPacket(id));
            await conn.SendPacketAsync(new ClientboundSetWordsPacket(indices));
        }

        await Task.WhenAll(Session.Players.Select(SendTopicAndWordsAsync));
        await ChangeStateAndUpdateAsync(StateOfGame.ChoosingWord);
        _ = ChooseEmptyWordsForPlayersAsync();
    }

    public async Task RevealChosenWordEntryAsync(Guid guid)
    {
        if (CurrentState != StateOfGame.ChoosingFinal)
        {
            throw new InvalidOperationException("Attempted to reveal cards when it's not the time to do it");
        }

        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundRevealChosenWordEntryPacket(guid))));
    }

    public async Task ChooseFinalAsync(ServerPlayer player, int index)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChooseFinalAsync(player, index));
            return;
        }
        
        if (CurrentState != StateOfGame.ChoosingFinal)
        {
            throw new InvalidOperationException("Attempted to choose final card when it's not the time to do it");
        }

        if (CurrentPlayer != player)
        {
            throw new InvalidOperationException("Wrong player is choosing the final");
        }

        var chosen = CurrentChosenWords[index];
        var credit = chosen.Player;
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundSetFinalPacket(index))));

        await Task.Delay(1000);

        if (credit != null)
        {
            var score = credit.Score + 1;
            await ChangePlayerScoreAndUpdateAsync(credit, score);
        }

        var maxScore = Options.WinScore;
        var winner = Session.Players.FirstOrDefault(p => p.Score >= maxScore);
        if (winner != null)
        {
            await ChangeWinnerPlayerAndUpdateAsync(winner);
            await ChangeStateAndUpdateAsync(StateOfGame.WinResult);
            return;
        }

        await PrepareNextRoundAsync();
    }

    private async Task PrepareNextRoundAsync()
    {
        await ChangeStateAndUpdateAsync(StateOfGame.PrepareNextRound);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            var pIndex = CurrentPlayerIndex + 1;
            pIndex %= Session.Players.Count;

            if (pIndex == 0)
            {
                await ChangeRoundCycleCountAndUpdateAsync(RoundCycle + 1);
            }
            await ChangeCurrentPlayerIndexAndUpdateAsync(pIndex);
            await StartRoundAsync();
        });
    }

    private async Task ChangeRoundCycleCountAndUpdateAsync(int cycle)
    { 
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangeRoundCycleCountAndUpdateAsync(cycle));
            return;
        }

        RoundCycle = cycle;
        Logger.Verbose($"Current cycle count is now {cycle}");
        await Task.WhenAll(Session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundUpdateRoundCyclePacket(cycle))));
    }
}