using KaLib.Utils;
using Microsoft.AspNetCore.Mvc;

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
        var server = context.RequestServices.GetService<Server>()!;
        var info = context.Connection;
        var connection = new ServerToPlayerConnection(webSocket, info);
        server.Players.Add(connection.Player);
        Logger.Info($"A player is connecting from {info.RemoteIpAddress}:{info.RemotePort}...");

        connection.Disconnected += async e =>
        {
            await Task.Yield();
            var player = connection.Player;
            Logger.Info($"Player {player} connected from {info.RemoteIpAddress}:{info.RemotePort}: {e.Reason}");
            server.Players.Remove(player);
        };

        SpinWait.SpinUntil(() => !connection.IsConnected);
    }    
}