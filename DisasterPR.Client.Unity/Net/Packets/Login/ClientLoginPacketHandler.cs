using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using Mochi.Utils;

namespace DisasterPR.Client.Unity.Net.Packets.Login;

public class ClientLoginPacketHandler : IClientLoginPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientLoginPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }

    public void HandleAckLogin(ClientboundAckLoginPacket packet)
    {
        Logger.Verbose($"Player {packet.Name} ID is {packet.Id}");
        Player.Name = packet.Name;
        Player.Id = packet.Id;
        Connection.CurrentState = PacketState.Play;
    }

    public void HandleDisconnect(ClientboundDisconnectPacket packet)
    {
        Connection.HandleDisconnect(packet);
    }
}