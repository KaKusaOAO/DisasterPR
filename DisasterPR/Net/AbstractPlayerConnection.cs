using System.Net.WebSockets;
using DisasterPR.Net.Packets;

namespace DisasterPR.Net;

public abstract class AbstractPlayerConnection
{
    public WebSocket WebSocket { get; }
    public bool IsConnected { get; set; }
    public RawPacketIO RawPacketIO { get; }
    public PacketFlow ReceivingFlow { get; }
    protected Dictionary<PacketState, IPacketHandler> Handlers { get; } = new();
    public PacketState CurrentState { get; set; }

    protected AbstractPlayerConnection(WebSocket webSocket, PacketFlow receivingFlow)
    {
        WebSocket = webSocket;
        RawPacketIO = new RawPacketIO(webSocket);
        ReceivingFlow = receivingFlow;

        _ = RunEventLoopAsync();
    }
    
    private async Task RunEventLoopAsync()
    {
        while (IsConnected)
        {
            if (!Handlers.Any()) continue;

            var token = new CancellationToken();
            var protocol = ConnectionProtocol.OfState(CurrentState);
            var packets = await RawPacketIO.ReadPacketsAsync(protocol, ReceivingFlow, token);

            foreach (var packet in packets)
            {
                var handler = Handlers[CurrentState];
                await packet.HandleAsync(handler);
            }
        }
    }

    public async Task SendPacketAsync(IPacket packet, CancellationToken token)
    {
        var protocol = ConnectionProtocol.OfState(CurrentState);
        await RawPacketIO.SendPacketAsync(protocol, ReceivingFlow.Opposite(), packet, token);
    }

    public async Task SendPacketAsync(IPacket packet)
    {
        var token = new CancellationToken();
        await SendPacketAsync(packet, token);
    }
}