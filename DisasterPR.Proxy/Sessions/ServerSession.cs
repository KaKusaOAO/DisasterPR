using System.Text.Json;
using System.Text.Json.Nodes;
using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Proxy.Net.Firebase;
using DisasterPR.Sessions;
using Firebase.Database.Offline;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using KaLib.Utils;
using KaLib.Utils.Extensions;
using QueryExtensions = Firebase.Database.Query.QueryExtensions;

namespace DisasterPR.Proxy.Sessions;

public class ServerSession : Session<ISessionPlayer>, IDisposable
{
    public override CardPack? CardPack { get; set; } = new ConcatPackProvider(IPackProvider.Upstream).Make();

    public bool IsValid { get; set; } = true;
    
    public ServerGameState ServerGameState { get; set; }

    public override IGameState GameState
    {
        get => ServerGameState;
        set => ServerGameState = (ServerGameState) value;
    }

    private static readonly int[] _roomIds = Enumerable.Range(1000, 9000).ToArray();

    private bool _isLocal;

    public bool IsLocal
    {
        get => _isLocal;
        set => _isLocal = value;
    }

    public bool IsRemote
    {
        get => !_isLocal;
        set => _isLocal = !value;
    }

    private int _remotePlayerCount;
    public bool HasUpstreamPlayersUpdateOnce { get; private set; }

    private List<IDisposable> _disposables = new();
    
    public static int CreateNewRoomId()
    {
        var firebase = GameServer.Instance.FirebaseClient;
        var query = firebase.Child("RoomList");
        var json = JsonSerializer.Deserialize<JsonNode>(query.OnceAsJsonAsync().Result)!.AsObject();

        var ids = new List<int>();
        foreach (var (key, _) in json)
        {
            try
            {
                var id = int.Parse(key);
                if (id is < 1000 or > 9999) continue;
                ids.Add(id);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        var list = _roomIds.ToList();
        list.RemoveAll(i => ids.Contains(i));
        return list.Any() ? list.Shuffled().First() : throw new IndexOutOfRangeException("No room left");
    }

    public ServerSession(int roomId)
    {
        RoomId = roomId;
        GameState = new ServerGameState(this);
        Options.EnabledCategories.Add(CardPack.Categories.First());

        UpdateFromUpstream();
        SubscribeUpstreamEvents();
        _ = RunEventLoopAsync();
    }

    private void SubscribeUpstreamEvents()
    {
        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{RoomId}");
        var queue = firebase.Child($"RoomQueue/{RoomId}");

        _disposables.Add(new FieldObserver(room).StartObserving(OnRemoteRoomUpdated));
        _disposables.Add(new FieldObserver(queue).StartObserving(OnRemoteRoomQueueUpdated));
    }

    private void OnRemoteRoomQueueUpdated(string key, JsonObject? root)
    {
        var firebase = GameServer.Instance.FirebaseClient;
        Logger.Info("RoomQueue: " + key + "=" + root?[key]);
        if (IsRemote) return;

        var value = root![key];
        if (key == "排隊名單")
        {
            var list = value?.AsObject();
            if (list == null) return;

            var entry = firebase.Child($"RoomQueue/{RoomId}");
            if (!list.Any()) return;
            
            var data = new JsonObject
            {
                {"進場", list.First().Value!.GetValue<ulong>()}
            };
            entry.PatchAsync(JsonSerializer.Serialize(data)).Wait();

            var queue = firebase.Child($"RoomQueue/{RoomId}/排隊名單/{list.First().Key}");
            queue.PutAsync("null").Wait();

            return;
        }
        
        if (key == "退出排隊名單")
        {
            var list = value?.AsObject();
            if (list == null) return;

            var entry = firebase.Child($"RoomQueue/{RoomId}");
            var data = new JsonObject
            {
                {"退出進場", list.First().Value!.GetValue<ulong>() }
            };
            entry.PatchAsync(JsonSerializer.Serialize(data)).Wait();
            
            var queue = firebase.Child($"RoomQueue/{RoomId}/退出排隊名單/{list.First().Key}");
            queue.PutAsync("null").Wait();
            return;
        }
    }

    private void OnRemoteRoomUpdated(string key, JsonObject? root)
    {
        var firebase = GameServer.Instance.FirebaseClient;
        Logger.Info("Room: " + key + "=" + root?[key]);

        if (string.IsNullOrEmpty(key) && root == null)
        {
            foreach (var player in Players.ToList())
            {
                player.KickFromSessionAsync("房間已被解散。").Wait();
                PlayerLeaveAsync(player).Wait();
            }

            return;
        }

        var value = root![key];
        if (key == "人數")
        {
            _remotePlayerCount = value!.GetValue<int>();
            OnRemotePlayerCountUpdated();
            return;
        }

        if (key == "提交排隊" && IsLocal)
        {
            var list = value?.AsObject();
            if (list == null) return;
            if (!list.Any()) return;

            var item = list.First().Value.AsObject();
            
            var chosens = ServerGameState.CurrentChosenWords;
            var idx = chosens.Count + 1;
            var aCount = ServerGameState.CurrentTopic.AnswerCount;
            var player = item[$"誰的答案"]!.GetValue<int>();
            var e = firebase.Child($"Room/{RoomId}/提交排隊/{list.First().Key}");
            e.PutAsync("null").Wait();

            var room = firebase.Child($"Room/{RoomId}");
            var data = new JsonObject
            {
                {$"誰的答案{idx}", player},
                {"答案卡數量", idx}
            };
            
            for (var i = 0; i < aCount; i++)
            {
                var wordSrcKey = aCount > 1 ? $"答案{i+1}" : $"答案";
                var wordKey = aCount > 1 ? $"答案{idx}{i+1}" : $"答案{idx}";
                var wordSrc = item[wordSrcKey]!.GetValue<string>();
                data.Add(wordKey, wordSrc);
            }

            room.PatchAsync(JsonSerializer.Serialize(data)).Wait();

            return;
        }

        if (key.StartsWith("卡包") && IsRemote)
        {
            var index = int.Parse(key[2..]) - 1;
            var enabled = value!.GetValue<int>() == 1;
            var category = CardPack!.Categories[index];

            if (enabled)
            {
                Options.EnabledCategories.Add(category);
            }
            else
            {
                Options.EnabledCategories.Remove(category);
            }

            foreach (var p in Players)
            {
                p.UpdateSessionOptions(this);
            }

            return;
        }

        if (key.StartsWith("題目"))
        {
            var first = root["題目1"]?.GetValue<string>();
            var second = root["題目2"]?.GetValue<string>();
            var third = root["題目3"]?.GetValue<string>();

            if (first == "[]") first = "";
            if (second == "[]") second = "";
            if (third == "[]") third = "";

            var answerCount = root["題目狀態"]?.GetValue<int>();
            var topic = CardPack!.Topics.FirstOrDefault(t =>
            {
                var condition = t.AnswerCount == answerCount && t.Texts[0] == first && t.Texts[1] == second;
                if (t.AnswerCount == 2)
                {
                    condition = condition && t.Texts[2] == third;
                }

                return condition;
            });

            if (topic == null)
            {
                Logger.Warn("Cannot find that topic!");
                Logger.Warn("Requested topic: " + first + "____" + second + (answerCount == 2 ? "____" + third : ""));
            }
            else
            {
                ServerGameState.CurrentTopic = topic;
                var id = CardPack.GetTopicIndex(topic);

                foreach (var p in Players)
                {
                    p.UpdateCurrentTopicAsync(id);
                }
            }
        }

        if (key == "倒數時間")
        {
            var time = value!.GetValue<int>();
            Options.CountdownTimeSet = new CountdownTimeSet(time);
            foreach (var p in Players)
            {
                p.UpdateSessionOptions(this);
            }
        }

        if (key == "目標分數")
        {
            var goal = value!.GetValue<int>();
            Options.WinScore = goal;
            foreach (var p in Players)
            {
                p.UpdateSessionOptions(this);
            }
        }

        if (key == "狀態")
        {
            var realStatus = firebase.Child($"RoomList/{RoomId}");
            if (realStatus.OnceSingleAsync<int>().Result == 1) return;
            
            var data = value?.AsObject();
            var state = data?["狀態"]?.GetValue<int>() ?? -1;

            if (state == -1) return;

            // 1: Started
            if (state >= 1)
            {
                if (GameState.CurrentState is StateOfGame.Waiting or StateOfGame.WinResult)
                {
                    ServerGameState.CurrentState = StateOfGame.Started;
                    foreach (var p in Players)
                    {
                        p.UpdateSessionGameStateAsync(StateOfGame.Started);
                    }
                }
            }

            if (state == 2)
            {
                ServerGameState.CurrentState = StateOfGame.ChoosingTopic;
                var (left, right) = ServerGameState.GetNextCandidateTopics();

                foreach (var p in Players)
                {
                    p.UpdateSessionGameStateAsync(StateOfGame.ChoosingTopic);
                    
                    var a = CardPack!.GetTopicIndex(left);
                    var b = CardPack.GetTopicIndex(right);
                    p.UpdateCandidateTopicsAsync(a, b);
                }

                ServerGameState.CancelTimer();
                _ = ServerGameState.ChooseOtherRandomTopicAsync();
            }

            if (state == 3)
            {
                ServerGameState.CurrentState = StateOfGame.ChoosingWord;
                foreach (var p in Players)
                {
                    if (!p.IsRemotePlayer)
                    {
                        var words = new List<HoldingWordCardEntry>();
                        words.AddRange(p.HoldingCards.Where(w => w.IsLocked));
                        words.AddRange(p.CardPool.Items.Shuffled().Take(11)
                            .Select(w => new HoldingWordCardEntry(w, false)));

                        p.HoldingCards.Clear();
                        p.HoldingCards.AddRange(words.Take(11));
                        p.UpdateHoldingWordsAsync(p.HoldingCards).Wait();
                    }

                    p.UpdateSessionGameStateAsync(StateOfGame.ChoosingWord);
                }
                
                ServerGameState.CancelTimer();
                _ = ServerGameState.ChooseEmptyWordsForPlayersAsync();
            }

            if (state == 4)
            {
                var index = (root["提案卡"]?.GetValue<int>() ?? 0) - 1;
                if (index == -1) return;

                if (ServerGameState.CurrentState == StateOfGame.ChoosingFinal)
                {
                    ServerGameState.CancelTimer();
                    _ = ServerGameState.ChooseFinalAsync(ServerGameState.CurrentPlayer, index);
                }
            }

            if (state == 5)
            {
                // Current player
                {
                    var idx = (root["現在回合"]?.GetValue<int>() ?? 0) - 1;
                    if (idx == -1) return;

                    ServerGameState.CurrentPlayerIndex = idx;
                    foreach (var p in Players)
                    {
                        p.UpdateCurrentPlayerIndexAsync(idx);
                    }
                }

                // Player scores
                {
                    for (var i = 0; i < Players.Count; i++)
                    {
                        var score = root[$"成員{i+1}分數"]!.GetValue<int>();
                        Players[i].Score = score;

                        foreach (var p in Players)
                        {
                            p.UpdatePlayerScoreAsync(Players[i], score);
                        }
                    }
                }
            }
            return;
        }

        if (key == "提案卡")
        {
            var index = (value?.GetValue<int>() ?? 0) - 1;
            if (index == -1) return;

            var card = ServerGameState.CurrentChosenWords[index];
            foreach (var p in Players)
            {
                p.RevealChosenWordEntryAsync(card.Id);
            }
        }
        
        if (key == "答案卡數量")
        {
            var count = value?.GetValue<int>() ?? 0;
            var chosens = ServerGameState.CurrentChosenWords;

            while (count < chosens.Count)
            {
                chosens.Remove(chosens.Last());
            }

            while (count > chosens.Count)
            {
                var idx = chosens.Count + 1;
                var words = new List<WordCard>();
                var aCount = ServerGameState.CurrentTopic.AnswerCount;
                for (var i = 0; i < aCount; i++)
                {
                    var wordKey = aCount > 1 ? $"答案{idx}{i+1}" : $"答案{idx}";
                    var word = root[wordKey]!.GetValue<string>();

                    var card = word == EmptyWordCard.Instance.Label
                        ? EmptyWordCard.Instance
                        : CardPack!.Words.ToList().Find(w => w.Label == word)!;
                    if (card is not EmptyWordCard) words.Add(card);
                }
                
                var player = root[$"誰的答案{idx}"]!.GetValue<int>();
                var p = player == 0 ? null : Players[player - 1];
                var entry = new ServerChosenWordEntry(ServerGameState, p, words);
                ServerGameState.CurrentChosenWords.Add(entry);
                
                var ids = words.Select(w => CardPack!.GetWordIndex(w)).ToList();
                Task.WhenAll(Players.Select(p =>
                    p.AddChosenWordEntryAsync(entry.Id, entry.Player?.Id, ids))).Wait();
            }

            if (count == Players.Count - 1)
            {
                ServerGameState.CurrentState = StateOfGame.ChoosingFinal;
                foreach (var p in Players)
                {
                    p.UpdateSessionGameStateAsync(StateOfGame.ChoosingFinal);
                }
                
                ServerGameState.CancelTimer();
                _ = ServerGameState.SkipFinalAsync();
            }
        }
    }

    private void OnRemotePlayerCountUpdated()
    {
        Logger.Info("Player count updated, processing...");
        var firebase = GameServer.Instance.FirebaseClient;
        var list = firebase.Child($"Room/{RoomId}");
        var json = JsonSerializer.Deserialize<JsonObject>(list.OnceAsJsonAsync().Result)!;
        var count = _remotePlayerCount;

        while (count > Players.Count)
        {
            var i = Players.Count + 1;
            var nameKey = $"成員{i}暱稱";
            var idKey = $"成員{i}ID";
            
            var player = new RemotePlayer
            {
                Name = json[nameKey]!.GetValue<string>(),
                UpstreamId = json[idKey]!.ToJsonString()
            };

            PlayerJoinAsync(player).Wait();
            player.State = PlayerState.Ready;
            foreach (var p in Players)
            {
                p.UpdatePlayerStateAsync(player);
            }
        }
        
        while (count < Players.Count)
        {
            var i = 1;
            var copied = Players.ToArray();
            
            while (i <= count)
            {
                var idKey = $"成員{i}ID";
                var id = json[idKey]!.ToJsonString();
                if (copied[i - 1].UpstreamId != id)
                {
                    Logger.Info("Player left: " + copied[i - 1].Name);
                    PlayerLeaveAsync(copied[i - 1]).Wait();
                }

                i++;
            }

            while (count < Players.Count)
            {
                var p = Players.Last();
                PlayerLeaveAsync(p).Wait();
            }
        }

        HasUpstreamPlayersUpdateOnce = true;
    }

    private async Task RunEventLoopAsync()
    {
        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{RoomId}");
        
        Logger.Info($"Running event loop at room {RoomId}...");
        while (IsValid)
        {
            await Task.Delay(16);
            if (IsRemote) continue;
            
            // Enabled categories
            {
                var enabled = Options.EnabledCategories;
                var categories = CardPack!.Categories;

                for (var i = 0; i < categories.Length; i++)
                {
                    var f = enabled.Contains(categories[i]);
                    var v = f ? 1 : 0;
                    var data = new JsonObject
                    {
                        {$"卡包{i + 1}", v}
                    };
                    room.PatchAsync(JsonSerializer.Serialize(data)).Wait();
                }
            }

            // Countdown time
            {
                var data = new JsonObject
                {
                    {"倒數時間", Options.CountdownTimeSet.TopicChooseTime }
                };
                room.PatchAsync(JsonSerializer.Serialize(data)).Wait();
            }
            
            // Goal
            {
                var data = new JsonObject
                {
                    {"目標分數", Options.WinScore }
                };
                room.PatchAsync(JsonSerializer.Serialize(data)).Wait();
            }
        }
        
        Logger.Info($"Event loop at room {RoomId} is completed.");
    }

    public void RunOnHosted()
    {
        IsLocal = true;
        
        var firebase = GameServer.Instance.FirebaseClient;
        var list = firebase.Child("RoomList");
        var data = new JsonObject
        {
            {RoomId.ToString(), 1}
        };
        list.PatchAsync(JsonSerializer.Serialize(data));

        var room = firebase.Child($"Room/{RoomId}");
        data = new JsonObject
        {
            { "版本", 1.286 }
        };
        room.PatchAsync(JsonSerializer.Serialize(data));

        var time = firebase.Child($"RoomTime/{RoomId}");
        time.PutAsync(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
    }
    
    private async Task OnPlayerDisconnectedAsync(DisconnectedEventArgs _)
    {
        var players = Players.Where(p => !p.IsConnected);
        foreach (var player in players)
        {
            await PlayerLeaveAsync(player);
        }
    }

    public async Task SetAndUpdateCardPackAsync(CardPack pack)
    {
        CardPack = pack;
        Options.EnabledCategories.Clear();
        Options.EnabledCategories.Add(pack.Categories.First());
        
        await Task.WhenAll(Players.Select(async p =>
        {
            await p.SetCardPackAsync(pack);
            await p.UpdateSessionOptions(this);
        }));
    }

    public async Task<bool> CheckPlayerCanJoinAsync(ServerPlayer player)
    {
        if (GameState.CurrentState != StateOfGame.Waiting)
        {
            var aiPlayers = Players.Where(s => s is AIPlayer).Cast<AIPlayer>().ToList();
            var ai = aiPlayers.Find(a => a.OriginalName == player.Name);
            if (ai == null)
            {
                await player.Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomPlaying);
                return false;
            }

            player.Id = ai.Id;
            await player.Connection.SendPacketAsync(new ClientboundUpdatePlayerGuidPacket(ai.Id));
            
            var index = Players.FindIndex(a => a == ai);
            Players.Remove(ai);
            
            await player.SendJoinRoomSequenceAsync(this, index);
            Players.Insert(index, player);
        
            player.Disconnected += OnPlayerDisconnectedAsync;
            await Task.WhenAll(Players.Where(p => p != player).Select(async p =>
            {
                await p.OnReplaceSessionPlayerAsync(index, player);
                await player.OnOtherPlayerUpdateStateAsync(p);
            }));
            
            player.Score = ai.Score;
            await Task.WhenAll(Players.Select(async p =>
            {
                await p.UpdatePlayerScoreAsync(player, player.Score);
            }));

            player.Session = this;
            player.CardPool = ai.CardPool;
            player.HoldingCards.Clear();
            player.HoldingCards.AddRange(ai.HoldingCards);
            player.State = PlayerState.InGame;
            await Task.WhenAll(Players.Select(async p =>
            {
                await p.OnOtherPlayerUpdateStateAsync(player);
            }));
                    
            _ = SendAllCurrentStateToPlayerAsync(player);
            return false;
        }
            
        if (Players.Count >= Constants.SessionMaxPlayers)
        {
            await player.Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomFull);
            return false;
        }

        if (Players.Find(p => p.Id == player.Id) != null)
        {
            await player.Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.GuidDuplicate);
            return false;
        }

        return true;
    }

    private async Task SendAllCurrentStateToPlayerAsync(ISessionPlayer player)
    {
        var state = GameState.CurrentState;
        if (state != StateOfGame.Waiting && state != StateOfGame.WinResult)
        {
            // Send started state first so player can enter the game screen
            await player.UpdateSessionGameStateAsync(StateOfGame.Started);
            await Task.Delay(100);
        }

        await player.UpdateCurrentPlayerIndexAsync(GameState.CurrentPlayerIndex);
        await player.UpdateRoundCycleAsync(GameState.RoundCycle);

        if (state == StateOfGame.ChoosingTopic)
        {
            var candidates = GameState.CandidateTopics;
            if (candidates.HasValue)
            {
                var left = CardPack!.GetTopicIndex(candidates.Value.Left);
                var right = CardPack.GetTopicIndex(candidates.Value.Right);
                await player.UpdateCandidateTopicsAsync(left, right);
            }
            await player.UpdateSessionGameStateAsync(state);
        }

        if (state >= StateOfGame.ChoosingWord)
        {
            var topic = GameState.CurrentTopic;
            if (topic != null!)
            {
                var id = CardPack!.GetTopicIndex(topic);
                await player.UpdateCurrentTopicAsync(id);
            }

            await player.UpdateHoldingWordsAsync(player.HoldingCards);
            
            // Change to this state so words and topics get updated
            await player.UpdateSessionGameStateAsync(StateOfGame.ChoosingWord);
            if (state != StateOfGame.ChoosingWord)
            {
                await player.UpdateSessionGameStateAsync(state);
            }
            
            var hasRevealed = false;
            foreach (var chosen in ServerGameState.CurrentChosenWords)
            {
                var desc = "[" + chosen.Words.Select(w => w.Label).JoinStrings(", ") + "]";
                Logger.Info($"Sending chosen words: {desc}");
                await player.AddChosenWordEntryAsync(chosen.Id, chosen.PlayerId,
                    chosen.Words.Select(w => CardPack!.GetWordIndex(w)).ToList());
                
                if (chosen.IsRevealed)
                {
                    hasRevealed = true;
                    await player.RevealChosenWordEntryAsync(chosen.Id);
                }
            }

            if (hasRevealed)
            {
                await player.RevealChosenWordEntryAsync(ServerGameState.LastRevealedGuid!.Value);
            }
        }
    }

    public async Task PlayerJoinAsync(ISessionPlayer player, int? selfIndex = null)
    {
        if (!player.IsRemotePlayer)
        {
            var index = Players.Count + 1;
            var firebase = GameServer.Instance.FirebaseClient;
            var room = firebase.Child($"Room/{RoomId}");
            var data = new JsonObject
            {
                {$"成員{index}暱稱", player.Name},
                {$"成員{index}ID", Math.Abs(player.Id.GetHashCode())},
                {"人數", index}
            };
            await room.PatchAsync(JsonSerializer.Serialize(data));
        }
        
        await player.SendJoinRoomSequenceAsync(this, selfIndex);
        
        player.Disconnected += OnPlayerDisconnectedAsync;
        await Task.WhenAll(Players.Select(async p =>
        {
            await p.OnNewPlayerJoinedSessionAsync(player);
            await player.OnOtherPlayerUpdateStateAsync(p);
        }));

        player.State = PlayerState.Joining;
        player.Session = this;
        Players.Add(player);
        ServerGameState!.ShuffleTopicsAndWords();
    }
    
    public async Task KickPlayerAsync(ISessionPlayer player)
    {
        await player.KickFromSessionAsync(RoomDisconnectReason.Kicked);
        await PlayerLeaveAsync(player);
    }
    
    public async Task PlayerLeaveAsync(ISessionPlayer player)
    {
        Logger.Info($"{player.Name} is leaving...");
        
        if (!player.IsRemotePlayer)
        {
            var idx = Players.IndexOf(player) + 1;
            var firebase = GameServer.Instance.FirebaseClient;
            var room = firebase.Child($"Room/{RoomId}");

            var data = new JsonObject
            {
                {"人數", Players.Count - 1}
            };
            
            while (idx <= Players.Count)
            {
                data.Add($"成員{idx}暱稱", idx == Players.Count ? null : Players[idx].Name);
                data.Add($"成員{idx}ID", idx == Players.Count ? null : JsonNode.Parse(Players[idx].UpstreamId));
                idx++;
            }
            
            await room.PatchAsync(JsonSerializer.Serialize(data));
        }
        
        var state = ServerGameState.CurrentState;
        if (state is StateOfGame.Waiting or StateOfGame.WinResult)
        {
            await InternalPlayerLeaveAsync(player);
            return;
        }

        foreach (var p in Players)
        {
            await p.KickFromSessionAsync("遊戲因玩家中離無法繼續，房間已被解散。");
            await p.SendToastAsync("代理伺服器不支援 AI 玩家功能。");
        }
        Cleanup();
    }

    private bool _disbanded;

    public async Task DisbandAsync()
    {
        if (_disbanded) return;
        _disbanded = true;
        
        Logger.Info($"Disbanding session {RoomId}...");
        foreach (var player in Players.ToList())
        {
            await player.KickFromSessionAsync("房間已被解散。");
            await PlayerLeaveAsync(player);
        }
    }

    private async Task InternalPlayerLeaveAsync(ISessionPlayer player)
    {
        player.Disconnected -= OnPlayerDisconnectedAsync;
        player.Session = null;
        Players.Remove(player);
        
        if (Players.All(p => p.IsRemotePlayer) || (HostPlayer.IsRemotePlayer && IsLocal))
        {
            await DisbandAsync();
            Cleanup();
            return;
        }
        
        await Task.WhenAll(Players.Select(p => p.OnPlayerLeftSessionAsync(player)));
    }

    private void Cleanup()
    {
        Invalidate();
        GameServer.Instance.Sessions.Remove(RoomId);
        Dispose();
        Logger.Verbose($"Removed room #{RoomId}");
    }

    public void Invalidate()
    {
        IsValid = false;
    }

    public void Dispose()
    {
        foreach (var d in _disposables)
        {
            d?.Dispose();
        }
        
        _disposables.Clear();

        if (!IsLocal) return;
        
        Logger.Info("Cleaning up room at upstream...");
        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{RoomId}");
        room.PutAsync("null").Wait();
        
        var list = firebase.Child($"RoomList/{RoomId}");
        list.PutAsync("null").Wait();

        var time = firebase.Child($"RoomTime/{RoomId}");
        time.PutAsync("null").Wait();
        Logger.Info("Cleanup should be completed");
    }

    public void UpdateFromUpstream()
    {
        var firebase = GameServer.Instance.FirebaseClient;
        var room = firebase.Child($"Room/{RoomId}");
        var json = JsonSerializer.Deserialize<JsonObject>(room.OnceAsJsonAsync().Result);
        if (json == null) return;
        if (!json.ContainsKey("人數")) return;
        
        _remotePlayerCount = json["人數"]!.GetValue<int>();
        OnRemotePlayerCountUpdated();
    }
}