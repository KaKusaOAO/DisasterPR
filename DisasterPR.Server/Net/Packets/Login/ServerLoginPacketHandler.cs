using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;

namespace DisasterPR.Server.Net.Packets.Login;

public class ServerLoginPacketHandler : IServerLoginPacketHandler
{
    public ServerToPlayerConnection Connection { get; }
    public ServerPlayer Player => Connection.Player;

    public ServerLoginPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    public async Task HandleLoginAsync(ServerboundLoginPacket packet)
    {
        var version = Connection.ProtocolVersion;
        if (version > Constants.ProtocolVersion)
        {
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ServerTooOld));
            return;
        }

        if (version < Constants.ProtocolVersion)
        {
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(PlayerKickReason.ClientTooOld));
            return;
        }

        Player.Name = packet.PlayerName;
        await Connection.SendPacketAsync(new ClientboundAckPacket());
        Connection.CurrentState = PacketState.Play;
    }
}