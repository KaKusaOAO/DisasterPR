using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Events;
using DisasterPR.Exceptions;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils;

namespace DisasterPR.Client.Unity;

public class LocalPlayer : AbstractClientPlayer
{
    public PlayerToServerConnection Connection { get; }

    public override List<HoldingWordCardEntry> HoldingCards { get; } = new();

    public LocalPlayer(IWebSocket webSocket, string name) : base(name)
    {
        Connection = new PlayerToServerConnection(webSocket, this);
        Connection.WebSocket.OnOpen += InternalLogin;
    }

    public void Login()
    {
        Connection.Connect();
    }
    
    private void InternalLogin()
    {
        Logger.Verbose("Connection opened, sending handshake");
        Connection.SendPacket(new ServerboundHelloPacket(Constants.ProtocolVersion));
        Connection.CurrentState = PacketState.Login;
        Connection.SendPacket(new ServerboundLoginPacket(Name));
    }
}