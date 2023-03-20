using DisasterPR.Client.Events;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;

namespace DisasterPR.Client.Net.Packets.Login;

public class ClientLoginPacketHandler : IClientLoginPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientLoginPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }

    public Task HandleAckAsync(ClientboundAckPacket packet)
    {
        Connection.CurrentState = PacketState.Play;
        return Task.CompletedTask;
    }

    public Task HandleDisconnectAsync(ClientboundDisconnectPacket packet)
    {
        Game.Instance.InternalOnDisconnected(this, new DisconnectedEventArgs
        {
            Reason = packet.Reason,
            Message = packet.Message
        });
        return Task.CompletedTask;
    }
}