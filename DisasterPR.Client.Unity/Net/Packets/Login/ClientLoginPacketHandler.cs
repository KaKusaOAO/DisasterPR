using System.Net.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using KaLib.Utils;

namespace DisasterPR.Client.Unity.Net.Packets.Login;

public class ClientLoginPacketHandler : IClientLoginPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientLoginPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }

    public void HandleAck(ClientboundAckPacket packet)
    {
        Logger.Verbose($"Player {Player.Name} ID is {packet.Id}");
        Player.Id = packet.Id;
        Connection.CurrentState = PacketState.Play;
    }

    public void HandleDisconnect(ClientboundDisconnectPacket packet)
    {
        Connection.HandleDisconnect(packet);
    }
}