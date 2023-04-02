using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Events;

namespace DisasterPR.Client.Unity;

public class Game
{
    private static Game? _instance;
    public static Game Instance => _instance ??= new Game();

    public event DisconnectedEventDelegate Disconnected;
    public event PlayerChatEventDelegate ReceivedPlayerChat;

    public LocalPlayer? Player { get; set; }

    public void Init(GameOptions options)
    {
        Player = new LocalPlayer(options.WebSocket, options.PlayerName);
        Player.Connection.Disconnected += async e =>
        {
            await Task.Yield();
            Disconnected?.Invoke(e);
        };
    }

    public void LoginPlayer()
    {
        if (Player == null) return;
        
        Player.Login();
    }

    internal void InternalOnDisconnected(DisconnectedEventArgs args) => Disconnected?.Invoke(args);

    internal void InternalOnPlayerChat(PlayerChatEventArgs args) => ReceivedPlayerChat?.Invoke(args);
}

public class GameOptions
{
    public IWebSocket WebSocket { get; set; }
    public string PlayerName { get; set; }
}