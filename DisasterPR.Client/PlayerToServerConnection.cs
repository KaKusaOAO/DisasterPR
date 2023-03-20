using System.Net.WebSockets;
using DisasterPR.Client.Net.Packets.Login;
using DisasterPR.Client.Net.Packets.Play;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using KaLib.Utils;

namespace DisasterPR.Client;

public class PlayerToServerConnection : AbstractPlayerConnection
{
    public LocalPlayer Player { get; }
    public ClientWebSocket ClientWebSocket => (ClientWebSocket) WebSocket;

    public PlayerToServerConnection(LocalPlayer player) : base(new ClientWebSocket(), PacketFlow.Clientbound)
    {
        Player = player;
        Handlers.Add(PacketState.Login, new ClientLoginPacketHandler(this));
        Handlers.Add(PacketState.Play, new ClientPlayPacketHandler(this));
    }

    public async Task ConnectAsync(CancellationToken token)
    {
        if (IsConnected)
        {
            Logger.Warn("Try to login while already connected?");
            return;
        }
        
        var uri = Constants.ServerUri;
        Logger.Info($"Connecting to server {uri}...");
        await ClientWebSocket.ConnectAsync(uri, token);
        IsConnected = true;

        _ = RunEventLoopAsync();
    }

    public async Task ConnectAsync() => await ConnectAsync(new CancellationToken());
}