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
        Logger.Info($"A client is connecting: {Connection.WebSocket}");
        Connection.CurrentState = PacketState.Login;
    }
}