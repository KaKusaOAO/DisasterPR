using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils;

namespace DisasterPR.Server;

public class ServerSession : Session<ServerPlayer>
{
    private static int[] _roomIds = Enumerable.Range(1000, 9000).ToArray();
    private static int _occupiedRooms;
    private static SemaphoreSlim _lock = new(1, 1);

    public event Action Emptied;

    static ServerSession()
    {
        Logger.Info("Generating room IDs...");
        for (var i = 0; i < _roomIds.Length; i++)
        {
            var j = Server.Instance.Random.Next(_roomIds.Length);
            (_roomIds[i], _roomIds[j]) = (_roomIds[j], _roomIds[i]);
        }
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
    }
    
    public async Task KickPlayerAsync(ServerPlayer player)
    {
        await player.Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(RoomDisconnectReason.Kicked));
        await PlayerLeaveAsync(player);
    }
    
    public async Task PlayerLeaveAsync(ServerPlayer player)
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
}