using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Server.Net.Packets.Handshake;
using DisasterPR.Server.Net.Packets.Login;
using DisasterPR.Server.Net.Packets.Play;

namespace DisasterPR.Server;

public class ServerToPlayerConnection : AbstractPlayerConnection
{
    public ServerPlayer Player { get; }
    public int ProtocolVersion { get; set; }

    public ServerToPlayerConnection(WebSocket webSocket) : base(webSocket, PacketFlow.Serverbound)
    {
        Player = new ServerPlayer(this);
        Handlers.Add(PacketState.Handshake, new ServerHandshakePacketHandler(this));
        Handlers.Add(PacketState.Login, new ServerLoginPacketHandler(this));
        Handlers.Add(PacketState.Play, new ServerPlayPacketHandler(this));
    }
}