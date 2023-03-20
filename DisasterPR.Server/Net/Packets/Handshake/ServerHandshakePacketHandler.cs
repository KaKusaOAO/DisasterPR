using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Handshake;
using KaLib.Utils;

namespace DisasterPR.Server.Net.Packets.Handshake;

public class ServerHandshakePacketHandler : IServerHandshakePacketHandler
{
    public ServerToPlayerConnection Connection { get; }

    public ServerHandshakePacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    public async Task HandleHelloAsync(ServerboundHelloPacket packet)
    {
        await Task.Yield();
        Logger.Verbose($"The client is using protocol version {packet.Version}");
        Connection.ProtocolVersion = packet.Version;
        Connection.CurrentState = PacketState.Login;
    }
}