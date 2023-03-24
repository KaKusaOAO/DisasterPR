using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Server.Sessions;

public class ServerSession : Session<ISessionPlayer>
{
    public override CardPack? CardPack { get; set; } = IPackProvider.Default.Make();

    public bool IsValid { get; set; } = true;
    
    public ServerGameState ServerGameState { get; set; }

    public override IGameState GameState
    {
        get => ServerGameState;
        set => ServerGameState = (ServerGameState) value;
    }

    private static int[] _roomIds = Enumerable.Range(1000, 9000).ToArray();
    private static int _occupiedRooms;
    private static SemaphoreSlim _lock = new(1, 1);

    public event Action Emptied;

    static ServerSession()
    {
        Logger.Info("Generating and shuffling room IDs...");
        _roomIds.Shuffle();
    }

    public static int CreateNewRoomId()
    {
        _lock.Wait();
        try
        {
            var room = _roomIds[_occupiedRooms];
            _occupiedRooms++;
            return room;
        }
        finally
        {
            _lock.Release();
        }
    }

    public ServerSession(int roomId)
    {
        RoomId = roomId;
        GameState = new ServerGameState(this);
        Options.EnabledCategories.Add(CardPack.Categories.First());
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

            player.Session = this;
            player.CardPool = ai.CardPool;
            player.HoldingCards.Clear();
            player.HoldingCards.AddRange(ai.HoldingCards);
            await SendAllCurrentStateToPlayerAsync(player);

            player.State = PlayerState.InGame;
            await Task.WhenAll(Players.Select(async p =>
            {
                await p.OnOtherPlayerUpdateStateAsync(player);
            }));
                    
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
            await player.UpdateSessionGameStateAsync(state);
        }

        if (state == StateOfGame.ChoosingTopic)
        {
            var candidates = GameState.CandidateTopics;
            if (candidates.HasValue)
            {
                var left = CardPack!.GetTopicIndex(candidates.Value.Left);
                var right = CardPack.GetTopicIndex(candidates.Value.Right);
                await player.UpdateCandidateTopicsAsync(left, right);
            }
        }

        if (state >= StateOfGame.ChoosingWord)
        {
            var topic = GameState.CurrentTopic;
            if (topic != null!)
            {
                var id = CardPack!.GetTopicIndex(topic);
                await player.UpdateCurrentTopicAsync(id);
            }

            var holdings = player.HoldingCards.Select(w => CardPack!.GetWordIndex(w.Card)).ToList();
            await player.UpdateHoldingWordsAsync(holdings);

            foreach (var chosen in GameState.CurrentChosenWords)
            {
                await player.AddChosenWordEntryAsync(chosen.Id, chosen.PlayerId,
                    chosen.Words.Select(w => CardPack!.GetWordIndex(w)).ToList());
                
                if (chosen.IsRevealed)
                {
                    await player.RevealChosenWordEntryAsync(chosen.Id);
                }
            }
        }
    }

    public async Task PlayerJoinAsync(ISessionPlayer player, int? selfIndex = null)
    {
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
    }
    
    public async Task KickPlayerAsync(ISessionPlayer player)
    {
        await player.KickFromSessionAsync(RoomDisconnectReason.Kicked);
        await PlayerLeaveAsync(player);
    }
    
    public async Task PlayerLeaveAsync(ISessionPlayer player)
    {
        var state = ServerGameState.CurrentState;
        if (state is StateOfGame.Waiting or StateOfGame.WinResult)
        {
            await InternalPlayerLeaveAsync(player);
            return;
        }

        // Try to replace the player by an AI player
        var ai = new AIPlayer(player);
        var index = Players.IndexOf(player);
        Players[index] = ai;
        await Task.WhenAll(Players.Where(p => p != ai).Select(async p =>
        {
            await p.OnReplaceSessionPlayerAsync(index, ai);
            await p.OnOtherPlayerUpdateStateAsync(ai);
        }));
        
        await SendAllCurrentStateToPlayerAsync(ai);
    }

    private async Task InternalPlayerLeaveAsync(ISessionPlayer player)
    {
        player.Disconnected -= OnPlayerDisconnectedAsync;
        player.Session = null;
        Players.Remove(player);
        
        if (!Players.Any())
        {
            Common.AcquireSemaphore(_lock, () => _occupiedRooms--);
            Emptied?.Invoke();
            return;
        }
        
        await Task.WhenAll(Players.Select(p => p.OnPlayerLeftSessionAsync(player)));
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}