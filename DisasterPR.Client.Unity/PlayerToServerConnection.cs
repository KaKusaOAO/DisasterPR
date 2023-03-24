using System.Diagnostics;
using System.Net.WebSockets;
using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Client.Unity.Net.Packets.Login;
using DisasterPR.Client.Unity.Net.Packets.Play;
using DisasterPR.Events;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using KaLib.Utils;
using AbstractPlayerConnection = DisasterPR.Client.Unity.Net.AbstractPlayerConnection;

namespace DisasterPR.Client.Unity;

public class PlayerToServerConnection : AbstractPlayerConnection
{
    public LocalPlayer Player { get; }
    private bool _isOpened;

    public bool IsOpen => _isOpened && IsConnected;

    public PlayerToServerConnection(IWebSocket webSocket, LocalPlayer player) : base(webSocket)
    {
        Player = player;
        Handlers.Add(PacketState.Login, new ClientLoginPacketHandler(this));
        Handlers.Add(PacketState.Play, new ClientPlayPacketHandler(this));
    }

    public void Connect()
    {
        if (IsConnected)
        {
            Logger.Warn("Try to login while already connected?");
            return;
        }
        
        var uri = Constants.ServerUri;
        Logger.Info($"Connecting to server {uri}...");

        WebSocket.Connect();
        IsConnected = true;
    }
}