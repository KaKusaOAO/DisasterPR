using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Client.Sessions;

public delegate Task AsyncPlayerEventDelegate(AbstractClientPlayer player);

public class LocalSession : Session<AbstractClientPlayer>
{
    public bool IsValid { get; private set; } = true;

    public LocalGameState LocalGameState { get; set; }

    private AsyncEventHandler<AsyncPlayerEventDelegate> _playerJoined = new();
    private AsyncEventHandler<AsyncPlayerEventDelegate> _playerLeft = new();
    
    public event AsyncPlayerEventDelegate PlayerJoined
    {
        add => _playerJoined.AddHandler(value);
        remove => _playerJoined.RemoveHandler(value);
    }
    
    public event AsyncPlayerEventDelegate PlayerLeft
    {
        add => _playerLeft.AddHandler(value);
        remove => _playerLeft.RemoveHandler(value);
    }
    
    public event Action Invalidated;

    public LocalSession()
    {
        LocalGameState = new LocalGameState(this);
    }

    public override IGameState GameState
    {
        get => LocalGameState;
        set => LocalGameState = (LocalGameState)value;
    }

    public async Task RequestStartAsync()
    {
        await Game.Instance.Player!.Connection.SendPacketAsync(new ServerboundRequestRoomStartPacket());
    }

    public void Invalidate()
    {
        IsValid = false;
        Invalidated?.Invoke();
    }

    public async Task PlayerJoinAsync(AbstractClientPlayer player)
    {
        Logger.Info($"Player {player.Name} ({player.Id}) has joined this session.");
        player.State = PlayerState.Joining;
        Players.Add(player);
        await _playerJoined.InvokeAsync(async d => await d(player));
    }

    public async Task PlayerLeaveAsync(AbstractClientPlayer player)
    {
        Logger.Info($"Player {player.Name} ({player.Id}) has left this session.");
        Players.Remove(player);
        await _playerLeft.InvokeAsync(async d => await d(player));
    }
}