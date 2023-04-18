using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using Mochi.Utils;

namespace DisasterPR.Client.Net.Packets.Login;

public class ClientLoginPacketHandler : IClientLoginPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientLoginPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }

    public void HandleAckLogin(ClientboundAckLoginPacket packet)
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