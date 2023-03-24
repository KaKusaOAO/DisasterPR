using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Client.Unity.Sessions;

public delegate void PlayerEventDelegate(AbstractClientPlayer player);

public class LocalSession : Session<AbstractClientPlayer>
{
    public bool IsValid { get; private set; } = true;

    public LocalGameState LocalGameState { get; set; }

    public event PlayerEventDelegate PlayerJoined;
    public event PlayerEventDelegate PlayerLeft;
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

    public void RequestStart()
    {
        Game.Instance.Player!.Connection.SendPacket(new ServerboundRequestRoomStartPacket());
    }

    public void Invalidate()
    {
        IsValid = false;
        Invalidated?.Invoke();
    }

    public void PlayerJoin(AbstractClientPlayer player)
    {
        Logger.Info($"Player {player.Name} ({player.Id}) has joined this session.");
        player.State = PlayerState.Joining;
        Players.Add(player);
        PlayerJoined?.Invoke(player);
    }

    public void PlayerReplace(int index, AbstractClientPlayer player)
    {
        var oldPlayer = Players[index];
        Logger.Info($"Player {player.Name} ({player.Id}) has joined this session, " +
                    $"replacing old player {oldPlayer.Name} ({oldPlayer.Id}).");
        player.State = PlayerState.Joining;
        Players[index] = player;
        
        PlayerLeft?.Invoke(oldPlayer);
        PlayerJoined?.Invoke(player);
    }

    public void PlayerLeave(AbstractClientPlayer player)
    {
        Logger.Info($"Player {player.Name} ({player.Id}) has left this session.");
        Players.Remove(player);
        PlayerLeft?.Invoke(player);
    }
}