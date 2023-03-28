using System.Collections.Concurrent;
using System.Diagnostics;
using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Nbt;
using KaLib.Utils;
using KaLib.Utils.Extensions;
using ISession = DisasterPR.Sessions.ISession;
using SessionOptions = DisasterPR.Sessions.SessionOptions;

namespace DisasterPR.Server.Sessions;

public class ServerGameState : IGameState
{
    public ServerSession Session { get; }
    ISession IGameState.Session => Session;

    public StateOfGame CurrentState { get; set; }
    public int CurrentPlayerIndex { get; set; }

    public ISessionPlayer CurrentPlayer => Session.Players[CurrentPlayerIndex];
    IPlayer IGameState.CurrentPlayer => CurrentPlayer;

    public ISessionPlayer? WinnerPlayer { get; set; }
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
    public bool HasChosenFinal { get; private set; }

    private Thread _thread;
    public Guid? LastRevealedGuid { get; private set; }

    public ServerGameState(ServerSession session)
    {
        Session = session;

        _thread = new Thread(InternalEventLoopAsync)
        {
            Name = $"Session #{session.RoomId}",
            IsBackground = true
        };
        _thread.Start();
    }

    private void InternalEventLoopAsync()
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
                    action().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Caught exception in event loop!");
                    Logger.Warn(ex.ToString());
                }
            }
        }

        Logger.Warn("Event loop stopped!");
    }

    private void ShuffleTopicsAndWords()
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => ShuffleTopicsAndWords());
            return;
        }

        _topics = new ShuffledPool<TopicCard>(
            Session.CardPack!.FilteredTopicsByEnabledCategories(Session.Options.EnabledCategories));

        foreach (var player in Session.Players)
        {
            player.CardPool =
                new ShuffledPool<WordCard>(
                    Session.CardPack.FilteredWordsByEnabledCategories(Session.Options.EnabledCategories));
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
        await Task.WhenAll(Session.Players.Select(p => p.UpdateSessionGameStateAsync(state)));
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
        await Task.WhenAll(Session.Players.Select(p => p.UpdateCurrentPlayerIndexAsync(index)));
    }

    private async Task ChangePlayerScoreAndUpdateAsync(ISessionPlayer player, int score)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangePlayerScoreAndUpdateAsync(player, score));
            return;
        }

        player.Score = score;
        await Task.WhenAll(Session.Players.Select(p => p.UpdatePlayerScoreAsync(player, score)));
    }

    private async Task ChangeWinnerPlayerAndUpdateAsync(ISessionPlayer player)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChangeWinnerPlayerAndUpdateAsync(player));
            return;
        }

        WinnerPlayer = player;
        await Task.WhenAll(Session.Players.Select(p => p.UpdateWinnerPlayerAsync(player.Id)));
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
            p.State = PlayerState.InGame;
            await Task.WhenAll(Session.Players.Select(async p2 =>
            {
                await p2.OnOtherPlayerUpdateStateAsync(p);
            }));
            await ChangePlayerScoreAndUpdateAsync(p, 0);
        }

        await ChangeRoundCycleCountAndUpdateAsync(1);
        await Task.Delay(1500);
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

        var pack = Session.CardPack!;
        var left = pack.GetTopicIndex(topics.Left);
        var right = pack.GetTopicIndex(topics.Right);
        CandidateTopics = topics;

        await CurrentPlayer.UpdateCandidateTopicsAsync(left, right);
        await ChangeStateAndUpdateAsync(StateOfGame.ChoosingTopic);
        HasChosenFinal = false;
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

    public async Task ChooseWordAsync(ISessionPlayer player, List<HoldingWordCardEntry> cards)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChooseWordAsync(player, cards));
            return;
        }
        
        if (cards.Any(card => card.IsLocked))
        {
            Logger.Warn($"Player {player.Name} is choosing locked cards!");
            return;
        }

        if (cards.Any(card => !player.HoldingCards.Contains(card)))
        {
            Logger.Warn($"Player {player.Name} is choosing non-existing card!");
            return;
        }

        if (CurrentChosenWords.Any(c => c.Player == player))
        {
            Logger.Warn($"Player {player.Name} has already chosen their card!");
            return;
        }

        var entry = new ServerChosenWordEntry(this, player, cards.Select(c => c.Card).ToList());
        CurrentChosenWords.Add(entry);

        var pack = Session.CardPack;
        var words = cards.Select(w => pack!.GetWordIndex(w.Card)).ToList();
        await Task.WhenAll(Session.Players.Select(p =>
            p.AddChosenWordEntryAsync(entry.Id, entry.Player?.Id, words)));

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
    
    public int CurrentTime { get; private set; }

    private async Task<bool> WaitForTimerAsync(int time)
    {
        _cts = new CancellationTokenSource();

        void SendTimerUpdate()
        {
            CurrentTime = time;
            foreach (var player in Session.Players)
            {
                _ = player.UpdateTimerAsync(time);
            }
        }

        Logger.Info("Starting timer...");
        while (time > 0)
        {
            SendTimerUpdate();
            try
            {
                await Task.Delay(1000, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                return false;
            }

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
                    p.AddChosenWordEntryAsync(entry.Id, entry.PlayerId, new List<int>())));
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

        async Task SendTopicAndWordsAsync(ISessionPlayer p)
        {
            var pack = Session.CardPack!;
            var id = pack.GetTopicIndex(CurrentTopic);
            var words = new List<HoldingWordCardEntry>();
            words.AddRange(p.HoldingCards.Where(w => w.IsLocked));
            
            var shuffled = p.CardPool.Items.Shuffled().ToList();
            var newWords = new List<HoldingWordCardEntry>();
            newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Noun)
                .Take(5)
                .Select(w => new HoldingWordCardEntry(w, false)));
            newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Verb)
                .Take(4)
                .Select(w => new HoldingWordCardEntry(w, false)));
            newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Adjective)
                .Take(2)
                .Select(w => new HoldingWordCardEntry(w, false)));
            words.AddRange(newWords.Shuffled());
            
            p.HoldingCards.Clear();
            p.HoldingCards.AddRange(words.Take(11));

            await p.UpdateCurrentTopicAsync(id);
            await p.UpdateHoldingWordsAsync(p.HoldingCards);
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

        var chosen = CurrentChosenWords.Find(w => w.Id == guid);
        if (chosen == null)
        {
            throw new InvalidOperationException("Attempted to reveal a non-existing card");
        }
        
        Logger.Info($"Revealed card {chosen.Words.Select(w => w.Label).JoinStrings(", ")} " +
                    $"by {chosen.Player?.Name ?? "<null>"}");

        chosen.IsRevealed = true;
        LastRevealedGuid = guid;
        await Task.WhenAll(Session.Players.Select(p =>
            p.RevealChosenWordEntryAsync(guid)));
    }

    public async Task ChooseFinalAsync(ISessionPlayer player, int index)
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

        if (HasChosenFinal) return;
        HasChosenFinal = true;

        var chosen = CurrentChosenWords[index];
        var credit = chosen.Player;
        await Task.WhenAll(Session.Players.Select(p =>
            p.UpdateFinalWordCardAsync(index)));

        await Task.Delay(1000);

        if (credit != null)
        {
            var score = credit.Score + 1;
            credit.Score = score;
            await ChangePlayerScoreAndUpdateAsync(credit, score);

            var maxScore = Options.WinScore;
            if (credit.Score >= maxScore)
            {
                await ChangeWinnerPlayerAndUpdateAsync(credit);
                await ChangeStateAndUpdateAsync(StateOfGame.WinResult);
                await ChangeStateAndUpdateAsync(StateOfGame.Waiting);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(1500);

                    foreach (var ai in Session.Players.Where(p => p is AIPlayer).ToList())
                    {
                        await Session.PlayerLeaveAsync(ai);
                    }
                });
                return;
            }
        }

        await PrepareNextRoundAsync();
    }

    private async Task PrepareNextRoundAsync()
    {
        await ChangeStateAndUpdateAsync(StateOfGame.PrepareNextRound);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            Logger.Verbose("Starting next round...");
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
        await Task.WhenAll(Session.Players.Select(p => p.UpdateRoundCycleAsync(cycle)));
    }
    
    public NbtCompound CreateSnapshot()
    {
        var tag = new NbtCompound();
        tag.AddOrSet("CurrentState", new NbtString(Enum.GetName(CurrentState)));
        tag.AddOrSet("CurrentPlayerIndex", new NbtInt(CurrentPlayerIndex));

        if (CurrentTopic != null!)
        {
            tag.AddOrSet("CurrentTopic", new NbtString(CurrentTopic.Texts.JoinStrings("____")));
        }

        if (CandidateTopics.HasValue)
        {
            var val = CandidateTopics.Value;
            var ct = new NbtCompound();
            ct.AddOrSet("Left", new NbtString(val.Left.Texts.JoinStrings("____")));
            ct.AddOrSet("Right", new NbtString(val.Right.Texts.JoinStrings("____")));
            tag.AddOrSet("CandidateTopics", ct);
        }

        var eList = new NbtList();
        foreach (var entry in CurrentChosenWords)
        {
            var et = new NbtCompound();
            et.AddOrSet("Player", new NbtString((entry.PlayerId ?? Guid.Empty).ToString()));

            var wList = new NbtList();
            foreach (var word in entry.Words)
            {
                var ct = new NbtCompound();
                ct.AddOrSet("Label", new NbtString(word.Label));
                ct.AddOrSet("Pos", new NbtString(Enum.GetName(word.PartOfSpeech)));
                wList.Add(ct);
            }

            et.AddOrSet("Cards", wList);
            eList.Add(et);
        }
        tag.AddOrSet("ChosenEntries", eList);

        tag.AddOrSet("Time", new NbtInt(CurrentTime));

        return tag;
    }
}