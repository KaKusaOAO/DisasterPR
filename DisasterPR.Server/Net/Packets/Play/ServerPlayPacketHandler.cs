using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Server.Net.Packets.Play;

public class ServerPlayPacketHandler : IServerPlayPacketHandler
{
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer Player => Connection.Player;

    public ServerPlayPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }
    
    public Task HandleChatAsync(ServerboundChatPacket packet)
    {
        throw new NotImplementedException();
    }

    public Task HandleHostRoomAsync(ServerboundHostRoomPacket packet)
    {
        throw new NotImplementedException();
    }

    public async Task HandleJoinRoomAsync(ServerboundJoinRoomPacket packet)
    {
        var sessions = Server.Instance.Sessions;
        var id = packet.RoomId;

        if (!sessions.ContainsKey(id))
        {
            await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.NotFound);
            return;
        }

        var session = sessions[id];
        await session.AcquireAsync(async () =>
        {
            if (session.Players.Count >= Constants.SessionMaxPlayers)
            {
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomFull);
                return;
            }

            session.JoinPlayer(Player);
        });
    }
}