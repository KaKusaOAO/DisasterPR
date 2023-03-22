using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Server.Sessions;

public class ServerSession : Session<ServerPlayer>
{
    public override CardPack? CardPack { get; set; } = CardPack.GetUpstreamAsync().Result;

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
            if (_occupiedRooms >= _roomIds.Length)
            {
                throw new IndexOutOfRangeException("Out of room numbers");
            }

            _occupiedRooms++;
            return _roomIds[_occupiedRooms - 1];
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
    
    public async Task PlayerJoinAsync(ServerPlayer player)
    {
        player.Connection.Disconnected += OnPlayerDisconnectedAsync;
        await Task.WhenAll(Players.Select(p => p.Connection.SendPacketAsync(new ClientboundAddPlayerPacket(player))));
        
        player.Session = this;
        Players.Add(player);

        _ = player.Connection.SendPacketAsync(new ClientboundSetCardPackPacket(CardPack));
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
            Players.Clear();
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