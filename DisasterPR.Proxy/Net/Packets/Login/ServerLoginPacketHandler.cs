using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils;
using LogLevel = KaLib.Utils.LogLevel;

namespace DisasterPR.Proxy.Net.Packets.Login;

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
        var shouldDisconnect = false;
        
        try
        {
            if (version > Constants.ProtocolVersion)
            {
                shouldDisconnect = true;
                Logger.Warn("The server is too old!");
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ServerTooOld));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }

            if (version < Constants.ProtocolVersion)
            {
                shouldDisconnect = true;
                Logger.Warn("The client is too old!");
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ClientTooOld));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }
        }
        catch (Exception)
        {
            // Why exceptions???
            if (shouldDisconnect) return;
        }

        Player.Name = packet.PlayerName;
        Logger.Verbose($"Player {Player.Name} ID is {Player.Id}");
        await Connection.SendPacketAsync(new ClientboundAckPacket(Player.Id));
        Connection.CurrentState = PacketState.Play;

        await Connection.SendPacketAsync(new ClientboundSystemChatPacket("您現在連線到的是以官方為後端的代理版本！", LogLevel.Error));
    }
}