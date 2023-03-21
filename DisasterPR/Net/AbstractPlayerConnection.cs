using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Play;
using KaLib.Texts;
using KaLib.Utils;

namespace DisasterPR.Net;

public delegate Task ReceivedPacketEventAsyncDelegate(ReceivedPacketEventArgs e);

public abstract class AbstractPlayerConnection
{
    public WebSocket WebSocket { get; }
    public bool IsConnected { get; set; }
    public RawPacketIO RawPacketIO { get; }
    public PacketFlow ReceivingFlow { get; }
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

    protected AbstractPlayerConnection(WebSocket webSocket, PacketFlow receivingFlow)
    {
        WebSocket = webSocket;
        RawPacketIO = new RawPacketIO(webSocket);
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
            await Task.Yield();
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
                await Task.Yield();
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
                    var id = stream.ReadVarInt();
                    var protocol = ConnectionProtocol.OfState(CurrentState);
                    var packet = protocol.CreatePacket(ReceivingFlow, id, stream);

                    var handler = Handlers[CurrentState];
                    await packet.HandleAsync(handler);
                    
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

        Logger.Info("Disposing WebSocket");
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
    }

    public async Task SendPacketAsync(IPacket packet) => await SendPacketAsync(packet, CancellationToken.None);
}