using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using Mochi.Utils;

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

        var name = ServerPlayer.ProcessPlayerName(packet.PlayerName);
        // Maybe we can process the name here
        Player.Name = name;
        
        Logger.Verbose($"Player {name} ID is {Player.Id}");
        await Connection.SendPacketAsync(new ClientboundAckLoginPacket(name, Player.Id));
        Connection.CurrentState = PacketState.Play;
    }
}