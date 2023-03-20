using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Client.Net.Packets.Play;

public class ClientPlayPacketHandler : IClientPlayPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientPlayPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }
    
    public Task HandleAddPlayerAsync(ClientboundAddPlayerPacket packet)
    {
        throw new NotImplementedException();
    }

    public Task HandleRemovePlayerAsync(ClientboundRemovePlayerPacket packet)
    {
        throw new NotImplementedException();
    }

    public Task HandleSessionStartAsync(ClientboundSessionStartPacket packet)
    {
        throw new NotImplementedException();
    }

    public Task HandleChatAsync(ClientboundChatPacket packet)
    {
        throw new NotImplementedException();
    }

    public Task HandleRoomDisconnectedAsync(ClientboundRoomDisconnectedPacket packet)
    {
        Player.Session = null;
        return Task.CompletedTask;
    }

    public Task HandleHeartbeatAsync(ClientboundHeartbeatPacket packet) => Task.CompletedTask;
    
    public Task HandleJoinedRoomAsync(ClientboundJoinedRoomPacket packet)
    {
        var session = new LocalSession();
        session.Players.AddRange(packet.Players.Select(p => new RemotePlayer(p)
        {
            Session = session
        }));
        
        session.Players.Add(Player);
        session.RoomId = packet.RoomId;
        
        Player.Session = session;
        return Task.CompletedTask;
    }
}