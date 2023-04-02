using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisasterPR.Server.Commands;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Server;

public class DashboardClient
{
    private MemoryStream _buffer = new();
    public Dashboard Dashboard { get; }
    public WebSocket WebSocket { get; }

    public DashboardClient(Dashboard dashboard, WebSocket webSocket)
    {
        Dashboard = dashboard;
        WebSocket = webSocket;
        _ = RunEventLoopAsync();
    }

    private async Task RunEventLoopAsync()
    {
        while (!WebSocket.CloseStatus.HasValue)
        {
            try
            {
                await Task.Delay(16);
                if (WebSocket.State != WebSocketState.Open) break;

                await ReceiveMessageAsync();
                if (WebSocket.CloseStatus.HasValue) break;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
        }

        Logger.Info("Disposing WebSocket");
        WebSocket.Dispose();
    }

    public async Task ReceiveMessageAsync()
    {
        var buffer = new byte[4096];
        var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        _buffer.Write(buffer, 0, result.Count);

        var pos = _buffer.Position;
        _buffer.Position = 0;
        var shouldRestore = true;
        try
        {
            var content = await JsonSerializer.DeserializeAsync<JsonObject>(_buffer);
            var buf = new MemoryStream();
            await _buffer.CopyToAsync(buf);
            _buffer = buf;
            shouldRestore = false;

            var cmd = content!["command"]!.GetValue<string>();
            _ = Command.ExecuteCommandByConsoleAsync(cmd);
        }
        catch (JsonException)
        {
            // ignored
        }
        finally
        {
            if (shouldRestore)
            {
                _buffer.Position = pos;
            }
        }
        
        if (WebSocket.CloseStatus.HasValue)
        {
            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }

    public async Task SendMessageAsync(LoggerEventArgs e)
    {
        await Task.Run(async () =>
        {
            var payload = new JsonObject
            {
                { "level", Enum.GetName(e.Level)!.ToLower() },
                { "tag", e.Tag.ToJson() },
                { "color", "#" + e.TagColor.Color.RGB.ToString("x6") },
                { "content", e.Content.ToJson() },
                { "timestamp", e.Timestamp.ToString("s") }
            };

            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            await WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
        });
    }
}