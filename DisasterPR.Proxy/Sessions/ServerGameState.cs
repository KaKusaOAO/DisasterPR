using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Firebase.Database.Query;
using KaLib.Utils;
using ISession = DisasterPR.Sessions.ISession;
using SessionOptions = DisasterPR.Sessions.SessionOptions;

namespace DisasterPR.Proxy.Sessions;

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

    public void ShuffleTopicsAndWords()
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => ShuffleTopicsAndWords());
            return;
        }

        Logger.Info("Shuffling topics and words...");
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

        if (Session.IsLocal)
        {
            var firebase = GameServer.Instance.FirebaseClient;
            var room = firebase.Child($"Room/{Session.RoomId}");
            var data = new JsonObject
            {
                {
                    "狀態", new JsonObject
                    {
                        {"狀態", 4}
                    }
                },
                {"現在回合", 1}
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
            
            data = new JsonObject
            {
                {
                    "狀態", new JsonObject
                    {
                        {"狀態", 5}
                    }
                }
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
            
            data = new JsonObject
            {
                {
                    "狀態", new JsonObject
                    {
                        {"狀態", 1}
                    }
                }
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
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

        (TopicCard Left, TopicCard Right) topics = GetNextCandidateTopics();

        var pack = Session.CardPack!;
        var left = pack.GetTopicIndex(topics.Left);
        var right = pack.GetTopicIndex(topics.Right);
        CandidateTopics = topics;

        await CurrentPlayer.UpdateCandidateTopicsAsync(left, right);
        await ChangeStateAndUpdateAsync(StateOfGame.ChoosingTopic);
        HasChosenFinal = false;
        _ = ChooseOtherRandomTopicAsync();
    }

    public (TopicCard Left, TopicCard Right) GetNextCandidateTopics()
    {
        var result = (_topics.Next(), _topics.Next());
        CandidateTopics = result;
        return result;
    }

    public async Task ChooseOtherRandomTopicAsync()
    {
        // Called when timed out
        var time = Options.CountdownTimeSet.TopicChooseTime;
        if (!await WaitForTimerAsync(time)) return;
        if (Session.IsRemote) return;

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

        var firebase = GameServer.Instance.FirebaseClient;
        var tsid = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{player.UpstreamId}";
        var room = firebase.Child($"Room/{Session.RoomId}/提交排隊/{tsid}");
        var aCount = CurrentTopic.AnswerCount;
        var data = new JsonObject
        {
            {"誰的答案", Session.Players.IndexOf(player) + 1}
        };
        for (var i = 0; i < aCount; i++)
        {
            var wordKey = aCount > 1 ? $"答案{i+1}" : "答案";
            var word = cards[i].Card.Label;
            data.Add(wordKey, word);
        }

        await room.PatchAsync(JsonSerializer.Serialize(data));

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

    public async Task SkipFinalAsync()
    {
        var time = Options.CountdownTimeSet.FinalChooseTime;
        if (!await WaitForTimerAsync(time)) return;
        if (Session.IsRemote) return;
        
        await PrepareNextRoundAsync();
    }

    public void CancelTimer()
    {
        _cts.Cancel();
    } 

    public async Task<bool> WaitForTimerAsync(int time)
    {
        _cts = new CancellationTokenSource();

        void SendTimerUpdate()
        {
            foreach (var player in Session.Players)
            {
                _ = player.UpdateTimerAsync(time);
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

    public async Task ChooseEmptyWordsForPlayersAsync()
    {
        // Called when timed out
        var time = Options.CountdownTimeSet.AnswerChooseTime;
        if (!await WaitForTimerAsync(time)) return;
        if (Session.IsRemote) return;

        async Task RunChooseEmptyWordAsync()
        {
            var players = Session.Players
                .Where(p => p != CurrentPlayer)
                .Where(p => CurrentChosenWords.All(w => w.Player != p));

            var firebase = GameServer.Instance.FirebaseClient;
            var room = firebase.Child($"Room/{Session.RoomId}");
            var data = new JsonObject();
            
            foreach (var p in players)
            {
                var entry = new ServerChosenWordEntry(this, null, new List<WordCard>());
                CurrentChosenWords.Add(entry);

                await Task.WhenAll(Session.Players.Select(p =>
                    p.AddChosenWordEntryAsync(entry.Id, entry.PlayerId, new List<int>())));
                
                var idx = CurrentChosenWords.Count;
                var aCount = CurrentTopic.AnswerCount;
                data.Add($"誰的答案{idx}", 0);
                for (var i = 0; i < aCount; i++)
                {
                    var wordKey = aCount > 1 ? $"答案{idx}{i+1}" : $"答案{idx}";
                    data.Add(wordKey, EmptyWordCard.Instance.Label);
                }
            }
            
            data.Add("答案卡數量", CurrentChosenWords.Count);
            await room.PatchAsync(JsonSerializer.Serialize(data));

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
        HasChosenFinal = false;

        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{Session.RoomId}");
        var data = new JsonObject
        {
            {"題目狀態", topic.AnswerCount},
            {"題目2狀態", 1}, // ?
        };
        for (var i = 0; i < topic.Texts.Count; i++)
        {
            data.Add($"題目{i+1}", topic.Texts[i]);
        }
        await room.PatchAsync(JsonSerializer.Serialize(data));

        data = new JsonObject
        {
            {
                "狀態", new JsonObject
                {
                    {"狀態", 3}
                }
            }
        };
        await room.PatchAsync(JsonSerializer.Serialize(data));

        async Task SendTopicAndWordsAsync(ISessionPlayer p)
        {
            if (!p.IsRemotePlayer)
            {
                var pack = Session.CardPack!;
                var id = pack.GetTopicIndex(CurrentTopic);
                var words = new List<HoldingWordCardEntry>();
                words.AddRange(p.HoldingCards.Where(w => w.IsLocked));
                words.AddRange(p.CardPool.Items.Shuffled().Take(11)
                    .Select(w => new HoldingWordCardEntry(w, false)));

                p.HoldingCards.Clear();
                p.HoldingCards.AddRange(words.Take(11));

                await p.UpdateCurrentTopicAsync(id);
            }

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

        chosen.IsRevealed = true;
        LastRevealedGuid = guid;
        
        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{Session.RoomId}");
        var data = new JsonObject
        {
            {"提案卡", CurrentChosenWords.IndexOf(chosen) + 1},
            {"提案卡是誰的", chosen.Player == null ? 0 : (Session.Players.IndexOf(chosen.Player) + 1)}
        };
        await room.PatchAsync(JsonSerializer.Serialize(data));
    }

    public async Task ChooseFinalAsync(ISessionPlayer player, int index)
    {
        if (Thread.CurrentThread != _thread)
        {
            _actions.Enqueue(async () => await ChooseFinalAsync(player, index));
            return;
        }

        if (!player.IsRemotePlayer)
        {
            if (HasChosenFinal) return;
            HasChosenFinal = true;
        }

        if (CurrentState != StateOfGame.ChoosingFinal)
        {
            throw new InvalidOperationException("Attempted to choose final card when it's not the time to do it");
        }

        if (CurrentPlayer != player)
        {
            throw new InvalidOperationException("Wrong player is choosing the final");
        }

        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{Session.RoomId}");
        var data = new JsonObject
        {
            {"狀態", new JsonObject
            {
                { "狀態", 4 }
            }},
            { "現在回合", (CurrentPlayerIndex + 1) % Session.Players.Count + 1}
        };
        await room.PatchAsync(JsonSerializer.Serialize(data));

        var chosen = CurrentChosenWords[index];
        var credit = chosen.Player;
        await Task.WhenAll(Session.Players.Select(p =>
            p.UpdateFinalWordCardAsync(index)));

        if (credit != null)
        {
            var score = credit.Score + 1;
            credit.Score = score;

            var idx = Session.Players.IndexOf(credit);
            data = new JsonObject
            {
                {$"成員{idx+1}分數", score }
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));

            await Task.Delay(1000);
            await ChangePlayerScoreAndUpdateAsync(credit, score);
            
            data = new JsonObject
            {
                {"狀態", new JsonObject
                {
                    { "狀態", 5 }
                }},
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
            
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
        else
        {
            await Task.Delay(1000);
            
            data = new JsonObject
            {
                {"狀態", new JsonObject
                {
                    { "狀態", 5 }
                }},
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
        }
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
}