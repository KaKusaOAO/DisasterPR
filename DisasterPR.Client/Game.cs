using DisasterPR.Events;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using KaLib.Utils;

namespace DisasterPR.Client;

public class Game
{
    private static Game? _instance;
    public static Game Instance => _instance ??= new Game();

    public event DisconnectedEventDelegate Disconnected;
    public event PlayerChatEventDelegate ReceivedPlayerChat;

    public LocalPlayer? Player { get; set; }

    public void Init(GameOptions options)
    {
        Player = new LocalPlayer(options.PlayerName);
        Player.Connection.Disconnected += async e =>
        {
            await Task.Yield();
            Disconnected?.Invoke(e);
        };
    }

    public async Task LoginPlayerAsync()
    {
        if (Player == null) return;
        
        await Player.LoginAsync();
        Logger.Verbose($"Player logged in as {Player.Name} ({Player.Id})");
    }

    public async Task HostRoomAsync()
    {
        if (Player == null) return;
        await Player.HostRoomAsync();
    }
    
    public async Task JoinRoomAsync(int roomId)
    {
        if (Player == null) return;
        await Player.JoinRoomAsync(roomId);
        Logger.Verbose($"Joined room: #{Player.Session?.RoomId}");
    }

    internal void InternalOnDisconnected(DisconnectedEventArgs args) => Disconnected?.Invoke(args);

    internal void InternalOnPlayerChat(PlayerChatEventArgs args) => ReceivedPlayerChat?.Invoke(args);
}

public class GameOptions
{
    public string PlayerName { get; set; }
}