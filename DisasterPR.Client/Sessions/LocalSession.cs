using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;

namespace DisasterPR.Client.Sessions;

public class LocalSession : Session<AbstractClientPlayer>
{
    public bool IsValid { get; private set; } = true;
    
    public LocalGameState LocalGameState { get; set; }

    public LocalSession()
    {
        LocalGameState = new LocalGameState(this);
    }

    public override IGameState GameState
    {
        get => LocalGameState;
        set => LocalGameState = (LocalGameState) value;
    }

    public async Task RequestStartAsync()
    {
        await Game.Instance.Player!.Connection.SendPacketAsync(new ServerboundRequestRoomStartPacket());
    }

    public void Invalidate()
    {
        IsValid = false;
    }
}