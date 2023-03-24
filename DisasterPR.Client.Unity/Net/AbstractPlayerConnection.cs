using System.Diagnostics;
using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using KaLib.Texts;
using KaLib.Utils;

namespace DisasterPR.Client.Unity.Net;

public delegate Task ReceivedPacketEventAsyncDelegate(ReceivedPacketEventArgs e);

public abstract class AbstractPlayerConnection
{
    public IWebSocket WebSocket { get; }
    public bool IsConnected { get; set; }
    public RawPacketIO RawPacketIO { get; }
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

    public event ReceivedPacketEventAsyncDelegate ReceivedPacket;
    public event DisconnectedEventDelegate Disconnected;

    private Stopwatch _stopwatch = new();
    private PacketState _currentState;

    protected AbstractPlayerConnection(IWebSocket webSocket)
    {
        WebSocket = webSocket;
        webSocket.OnClose += e =>
        {
            if (e == WebSocketCloseCode.Normal) return;
            
            Disconnected?.Invoke(new DisconnectedEventArgs
            {
                Reason = PlayerKickReason.Disconnected
            });
        };
        
        RawPacketIO = new RawPacketIO(webSocket);
        RawPacketIO.OnPacketReceived += RawPacketIOOnOnPacketReceived;
        _stopwatch.Start();
        
        TaskManager.Instance.AddTickable(() =>
        {
            Update();
            return !IsConnected;
        });
    }

    public void Update()
    {
        UpdateHeartbeat();
    }

    private void RawPacketIOOnOnPacketReceived(List<MemoryStream> packets)
    {
        foreach (var stream in packets)
        {
            var id = stream.ReadVarInt();
            var protocol = ConnectionProtocol.OfState(CurrentState);
            var packet = protocol.CreatePacket(PacketFlow.Clientbound, id, stream);
                    
            Logger.Verbose(TranslateText.Of("Received packet: %s")
                .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
                    
            var handler = Handlers[CurrentState];
            packet.Handle(handler);
                    
            ReceivedPacket?.Invoke(new ReceivedPacketEventArgs
            {
                Packet = packet
            });
        }
    }

    private void UpdateHeartbeat()
    {
        if (_stopwatch.Elapsed.TotalSeconds > 5 && CurrentState == PacketState.Play)
        {
            _stopwatch.Restart();
            SendPacket(new ServerboundHeartbeatPacket());
        }
    }

    public void SendPacket(IPacket packet)
    {
        if (WebSocket.GetState() != WebSocketState.Open)
        {
            IsConnected = false;
            return;
        }
        
        var protocol = ConnectionProtocol.OfState(CurrentState);
        RawPacketIO.SendPacket(protocol, PacketFlow.Serverbound, packet);
        
        Logger.Verbose(TranslateText.Of("Sent packet: %s")
            .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
    }

    public void HandleDisconnect(ClientboundDisconnectPacket packet)
    {
        Disconnected?.Invoke(new DisconnectedEventArgs()
        {
            Reason = packet.Reason,
            Message = packet.Message
        });
    }
}