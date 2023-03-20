using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Client.Net.Packets.Play;

public class ClientPlayPacketHandler : IClientPlayPacketHandler
{
    private PlayerToServerConnection _connection;
    
    public ClientPlayPacketHandler(PlayerToServerConnection connection)
    {
        _connection = connection;
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
        throw new NotImplementedException();
    }
}