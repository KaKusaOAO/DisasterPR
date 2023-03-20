using DisasterPR.Client.Events;

namespace DisasterPR.Client;

public class Game
{
    private static Game? _instance;
    public static Game Instance => _instance ??= new Game();

    public event DisconnectedEventDelegate Disconnected;

    public LocalPlayer? Player { get; set; }

    public void Init(GameOptions options)
    {
        Player = new LocalPlayer(options.PlayerName);
    }

    public async Task LoginAsync()
    {
        if (Player == null) return;
        await Player.LoginAsync();
    }

    internal void InternalOnDisconnected(object sender, DisconnectedEventArgs args) => 
        Disconnected?.Invoke(sender, args);
}

public class GameOptions
{
    public string PlayerName { get; set; }
}