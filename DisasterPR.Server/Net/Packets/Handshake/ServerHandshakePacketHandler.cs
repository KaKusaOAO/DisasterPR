using DisasterPR.Net;
using DisasterPR.Net.Packets.Handshake;
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
    }
}