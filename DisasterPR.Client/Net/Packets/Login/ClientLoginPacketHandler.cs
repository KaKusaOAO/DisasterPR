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

    public void HandleAck(ClientboundAckPacket packet)
    {
        Logger.Verbose($"Player {Player.Name} ID is {packet.Id}");
        Player.Id = packet.Id;
        Connection.CurrentState = PacketState.Play;
    }

    public async void HandleDisconnect(ClientboundDisconnectPacket packet)
    {
        await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }
}