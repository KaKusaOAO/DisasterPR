using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace DisasterPR.Server.Controllers;

public class DashboardController : ControllerBase
{
    [Route("/api/dashboard")]
    public async Task Get()
    {
        var context = HttpContext;
        if (!(context.Connection.RemoteIpAddress?.Equals(context.Connection.LocalIpAddress) ?? false))
        {
            context.Response.StatusCode = 403;
            return;
        }
        
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var dashboard = GameServer.Instance.Dashboard;
        var client = new DashboardClient(dashboard, webSocket);
        dashboard.AddClient(client);
        SpinWait.SpinUntil(() => webSocket.CloseStatus.HasValue);
        dashboard.RemoveClient(client);
    }
}