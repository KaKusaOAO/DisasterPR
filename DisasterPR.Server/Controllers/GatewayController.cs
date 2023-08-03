using DisasterPR.Net.Packets;
using Microsoft.AspNetCore.Mvc;
using Mochi.Utils;

namespace DisasterPR.Server.Controllers;

public class GatewayController : ControllerBase
{
    [Route("/gateway")]
    public async Task Get()
    {
        var context = HttpContext;
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var server = GameServer.Instance;
        var info = context.Connection;

        var type = PacketContentType.Binary;
        if (context.Request.Query.TryGetValue("encoding", out var encoding) && encoding == "json")
        {
            type = PacketContentType.Json;
        }
        
        var connection = new ServerToPlayerConnection(webSocket, info, type);
        server.Players.Add(connection.Player);
        Logger.Info($"A player is connecting from {info.RemoteIpAddress}:{info.RemotePort}...");

        connection.Disconnected += async e =>
        {
            await Task.Yield();
            var player = connection.Player;
            Logger.Info($"Player {player.Name} disconnected from {info.RemoteIpAddress}:{info.RemotePort}: {e.Reason}");
            server.Players.Remove(player);
        };

        SpinWait.SpinUntil(() => !connection.IsConnected);
    }
}