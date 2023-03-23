using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Server.Sessions;

public class ServerSession : Session<ServerPlayer>
{
    public override CardPack? CardPack { get; set; } = IPackProvider.Default.MakeBuilderAsync().Result.Build();

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
        var players = Players.Where(p => !p.Connection.IsConnected);
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
            await p.Connection.SendPacketAsync(new ClientboundSetCardPackPacket(pack));
            await p.Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(this));
        }));
    }
    
    public async Task PlayerJoinAsync(ServerPlayer player)
    {
        player.Connection.Disconnected += OnPlayerDisconnectedAsync;
        await Task.WhenAll(Players.Select(async p =>
        {
            await p.Connection.SendPacketAsync(new ClientboundAddPlayerPacket(player));
            await player.Connection.SendPacketAsync(new ClientboundUpdatePlayerStatePacket(p));
        }));

        player.State = PlayerState.Joining;
        player.Session = this;
        Players.Add(player);
    }
    
    public async Task KickPlayerAsync(ServerPlayer player)
    {
        await player.Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(RoomDisconnectReason.Kicked));
        await PlayerLeaveAsync(player);
    }
    
    public async Task PlayerLeaveAsync(ServerPlayer player)
    {
        var state = ServerGameState.CurrentState;
        if (state != StateOfGame.Waiting && state != StateOfGame.WinResult)
        {
            await Task.WhenAll(Players.Select(p => 
                p.Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(RoomDisconnectReason.SomeoneLeftWhileInGame))));
            
            foreach (var p in Players)
            {
                p.Session = null;
            }
            
            Players.Clear();
            Common.AcquireSemaphore(_lock, () => _occupiedRooms--);
            Emptied?.Invoke();
            return;
        }

        await InternalPlayerLeaveAsync(player);
    }

    private async Task InternalPlayerLeaveAsync(ServerPlayer player)
    {
        player.Connection.Disconnected -= OnPlayerDisconnectedAsync;
        player.Session = null;
        Players.Remove(player);
        
        if (!Players.Any())
        {
            Common.AcquireSemaphore(_lock, () => _occupiedRooms--);
            Emptied?.Invoke();
            return;
        }
        
        await Task.WhenAll(Players.Select(p => p.Connection.SendPacketAsync(new ClientboundRemovePlayerPacket(player))));
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}