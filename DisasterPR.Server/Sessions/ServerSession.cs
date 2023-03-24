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
    
    public async Task PlayerJoinAsync(ISessionPlayer player)
    {
        await player.SendJoinRoomSequenceAsync(this);
        
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
        await Task.WhenAll(Players.Select(async p =>
        {
            await p.OnReplaceSessionPlayerAsync(index, ai);
            await p.OnOtherPlayerUpdateStateAsync(ai);
        }));
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