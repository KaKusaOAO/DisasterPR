using DisasterPR.Net;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;

namespace DisasterPR.Client;

public class LocalPlayer : AbstractClientPlayer
{
    public PlayerToServerConnection Connection { get; }

    public LocalPlayer(string name) : base(name)
    {
        Connection = new PlayerToServerConnection(this);
    }

    public async Task LoginAsync()
    {
        await Connection.ConnectAsync();
        await Connection.SendPacketAsync(new ServerboundHelloPacket(Constants.ProtocolVersion));
        Connection.CurrentState = PacketState.Login;

        await Connection.SendPacketAsync(new ServerboundLoginPacket(Name));
    }
}