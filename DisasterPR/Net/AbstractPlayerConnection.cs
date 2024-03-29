using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DisasterPR.Events;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Play;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Net;

public delegate Task ReceivedPacketEventAsyncDelegate(ReceivedPacketEventArgs e);

public abstract class AbstractPlayerConnection
{
    public WebSocket WebSocket { get; }
    public bool IsConnected { get; set; }
    public RawPacketIO RawPacketIO { get; }
    public PacketFlow ReceivingFlow { get; }
    public PacketContentType ContentType { get; }
    protected Dictionary<PacketState, IPacketHandler> Handlers { get; } = new();

    public PacketState CurrentState
    {
        get => _currentState;
        set
        {
            Logger.Verbose($"Current state updated to {value}");
            _currentState = value;
        }
    }

    public PacketStream PacketStream { get; }

    private readonly AsyncEventHandler<ReceivedPacketEventAsyncDelegate> _receivedPacketEvent = new();

    private readonly AsyncEventHandler<DisconnectedEventDelegate> _disconnectedEvent = new();

    public event ReceivedPacketEventAsyncDelegate ReceivedPacket
    {
        add => _receivedPacketEvent.AddHandler(value);
        remove => _receivedPacketEvent.RemoveHandler(value);
    }
    
    public event DisconnectedEventDelegate Disconnected
    {
        add => _disconnectedEvent.AddHandler(value);
        remove => _disconnectedEvent.RemoveHandler(value);
    }

    private Stopwatch _stopwatch = new();
    private PacketState _currentState;

    protected AbstractPlayerConnection(WebSocket webSocket, PacketFlow receivingFlow, 
        PacketContentType contentType = PacketContentType.Binary)
    {
        ContentType = contentType;
        WebSocket = webSocket;
        RawPacketIO = new RawPacketIO(webSocket, contentType);
        ReceivingFlow = receivingFlow;
        PacketStream = new PacketStream(this);

        if (receivingFlow != PacketFlow.Serverbound) return;
        IsConnected = true;
        _ = RunEventLoopAsync();
    }

    private async Task RunHeartbeatAsync()
    {
        _stopwatch.Start();
        while (IsConnected)
        {
            await Task.Delay(2500);

            if (_stopwatch.Elapsed.TotalSeconds > 5 && CurrentState == PacketState.Play)
            {
                _stopwatch.Restart();
                var flow = ReceivingFlow.Opposite();
                var packet = flow == PacketFlow.Clientbound
                    ? new ClientboundHeartbeatPacket()
                    : new ServerboundHeartbeatPacket() as IPacket;
                await SendPacketAsync(packet);
            }
        }
    }
    
    protected async Task RunEventLoopAsync()
    {
        Logger.Verbose("Starting event loop...");
        _ = RunHeartbeatAsync();
        
        while (IsConnected)
        {
            try
            {
                await Task.Delay(16);
                if (WebSocket.State != WebSocketState.Open)
                {
                    IsConnected = false;
                    break;
                }
                
                if (!Handlers.Any()) continue;

                var packets = await RawPacketIO.ReadRawPacketsAsync(CancellationToken.None);
                if (WebSocket.CloseStatus.HasValue)
                {
                    IsConnected = false;
                    break;
                }
                
                foreach (var stream in packets)
                {
                    var protocol = ConnectionProtocol.OfState(CurrentState);

                    IPacket packet;
                    if (ContentType == PacketContentType.Binary)
                    {
                        var id = stream.ReadVarInt();
                        packet = protocol.CreatePacket(ReceivingFlow, id, stream);
                    } else if (ContentType == PacketContentType.Json)
                    {
                        var payload = JsonSerializer.Deserialize<JsonObject>(stream.Stream)!;
                        var id = payload["op"]!.GetValue<int>();
                        var data = payload["d"]!.AsObject();
                        packet = protocol.CreatePacket(ReceivingFlow, id, data);
                    }
                    else
                    {
                        throw new Exception("Unknown content type!");
                    }

                    // Logger.Verbose(TranslateText.Of("Received packet: %s")
                    //     .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));

                    try
                    {
                        var handler = Handlers[CurrentState];
                        packet.Handle(handler);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex.ToString());
                    }

                    await _receivedPacketEvent.InvokeAsync(async e =>
                    {
                        await e(new ReceivedPacketEventArgs
                        {
                            Packet = packet
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
        }

        // Logger.Info("Disposing WebSocket");
        WebSocket.Dispose();
        await _disconnectedEvent.InvokeAsync(async e =>
        {
            await e(new DisconnectedEventArgs
            {
                Reason = PlayerKickReason.Disconnected
            });
        });
    }

    public Task<IPacket> GetNextPacketAsync() => PacketStream.GetNextPacketAsync();

    public async Task SendPacketAsync(IPacket packet, CancellationToken token)
    {
        if (WebSocket.State != WebSocketState.Open)
        {
            IsConnected = false;
            return;
        }
        
        var protocol = ConnectionProtocol.OfState(CurrentState);
        await RawPacketIO.SendPacketAsync(protocol, ReceivingFlow.Opposite(), packet, token);
        
        // Logger.Verbose(TranslateText.Of("Sent packet: %s")
        //     .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
    }

    public async Task SendPacketAsync(IPacket packet) => await SendPacketAsync(packet, CancellationToken.None);
}