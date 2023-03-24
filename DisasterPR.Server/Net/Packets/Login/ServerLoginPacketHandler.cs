using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using KaLib.Utils;

namespace DisasterPR.Server.Net.Packets.Login;

public class ServerLoginPacketHandler : IServerLoginPacketHandler
{
    public ServerToPlayerConnection Connection { get; }
    public ServerPlayer Player => Connection.Player;

    public ServerLoginPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    public async void HandleLogin(ServerboundLoginPacket packet)
    {
        var version = Connection.ProtocolVersion;
        if (version > Constants.ProtocolVersion)
        {
            Logger.Warn("The server is too old!");
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ServerTooOld));
            await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            return;
        }

        if (version < Constants.ProtocolVersion)
        {
            Logger.Warn("The client is too old!");
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ClientTooOld));
            await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            return;
        }

        Player.Name = packet.PlayerName;
        Logger.Verbose($"Player {Player.Name} ID is {Player.Id}");
        await Connection.SendPacketAsync(new ClientboundAckPacket(Player.Id));
        Connection.CurrentState = PacketState.Play;
    }
}