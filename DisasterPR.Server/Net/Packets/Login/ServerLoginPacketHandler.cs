using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using Mochi.Texts;
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

        async Task DisconnectAsync(PlayerKickReason reason)
        {
            shouldDisconnect = true;
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(reason));
            await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        
        try
        {
            if (version > Constants.ProtocolVersion)
            {
                Logger.Warn("The server is too old!");
                await DisconnectAsync(PlayerKickReason.ServerTooOld);
                return;
            }

            if (version < Constants.ProtocolVersion)
            {
                Logger.Warn("The client is too old!");
                await DisconnectAsync(PlayerKickReason.ClientTooOld);
                return;
            }

            if (!PlayerName.IsValid(packet.PlayerName))
            {
                Logger.Warn("The player name is invalid!");
                await DisconnectAsync(PlayerKickReason.InvalidName);
                return;
            }
        }
        catch (Exception ex)
        {
            // Discard exceptions if we have already disconnected
            if (shouldDisconnect) return;

            // Otherwise, log the exception because something is definitely broken
            Logger.Warn("Failed to validate player!");
            Logger.Warn(ex);
        }
        
        var name = PlayerName.ProcessName(packet.PlayerName);
        Player.Name = name;
        
        Logger.Verbose(TranslateText.Of("Player %s ID is %s")
            .AddWith(LiteralText.Of(name).SetColor(TextColor.Gold))
            .AddWith(LiteralText.Of(Player.Id.ToString()).SetColor(TextColor.Green))
        );
        await Connection.SendPacketAsync(new ClientboundAckLoginPacket(Player.Id, name));
        Connection.CurrentState = PacketState.Play;
        
        await Player.SendToastAsync($"歡迎，{name}！");
    }
}