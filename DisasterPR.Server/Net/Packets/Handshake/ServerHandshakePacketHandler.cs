using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Server.Net.Packets.Handshake;

public class ServerHandshakePacketHandler : IServerHandshakePacketHandler
{
    public ServerToPlayerConnection Connection { get; }

    public ServerHandshakePacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    public void HandleHello(ServerboundHelloPacket packet)
    {
        Logger.Verbose(TranslateText.Of("The client is using protocol version %s")
            .AddWith(LiteralText.Of(packet.Version.ToString()).SetColor(TextColor.Green))
        );
        Connection.ProtocolVersion = packet.Version;
        Connection.CurrentState = PacketState.Login;

        if (packet.Version < 6)
        {
            // Version 6: Start supporting different login types. (eg. Discord)
            // The login packet is incompatible with the current protocol.
            // We must disconnect them now so we don't get a malformed login packet.
            Task.Run(async () =>
            {
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ClientTooOld));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }).Wait();
        }
    }
}