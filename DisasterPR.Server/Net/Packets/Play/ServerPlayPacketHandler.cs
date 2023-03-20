using DisasterPR.Net.Packets.Play;
using KaLib.Utils;

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

    public async Task HandleHostRoomAsync(ServerboundHostRoomPacket packet)
    {
        var server = Server.Instance;
        var sessions = server.Sessions;

        try
        {
            var roomId = ServerSession.CreateNewRoomId();
            Logger.Verbose($"Created room #{roomId}");
            
            var session = new ServerSession
            {
                RoomId = roomId
            };
            session.Emptied += () =>
            {
                sessions.Remove(roomId);
                Logger.Verbose($"Removed room #{roomId}");
            };
            
            sessions.Add(roomId, session);
            await JoinSessionAsync(session);
        }
        catch (IndexOutOfRangeException)
        {
            await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.NoRoomLeft);
        }
    }

    private async Task JoinSessionAsync(ServerSession session)
    {
        await session.AcquireAsync(async () =>
        {
            if (session.Players.Count >= Constants.SessionMaxPlayers)
            {
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomFull);
                return;
            }

            await Connection.SendPacketAsync(new ClientboundJoinedRoomPacket(session));
            await session.PlayerJoinAsync(Player);
        });
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
        await JoinSessionAsync(session);
    }

    public Task HandleHeartbeatAsync(ServerboundHeartbeatPacket packet) => Task.CompletedTask;
}