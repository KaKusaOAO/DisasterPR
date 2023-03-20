using System.Net.WebSockets;
using DisasterPR.Events;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using KaLib.Utils;

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
        Logger.Verbose($"Player {Player.Name} ID is {packet.Id}");
        Player.Id = packet.Id;
        Connection.CurrentState = PacketState.Play;
        return Task.CompletedTask;
    }

    public async Task HandleDisconnectAsync(ClientboundDisconnectPacket packet)
    {
        await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }
}