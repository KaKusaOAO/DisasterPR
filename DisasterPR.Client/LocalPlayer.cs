using DisasterPR.Events;
using DisasterPR.Exceptions;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils;

namespace DisasterPR.Client;

public class LocalPlayer : AbstractClientPlayer
{
    public PlayerToServerConnection Connection { get; }

    public LocalPlayer(string name) : base(name)
    {
        Connection = new PlayerToServerConnection(this);
    }

    public async Task LoginAsync()
    {
        await Connection.ConnectAsync();
        await Connection.SendPacketAsync(new ServerboundHelloPacket(Constants.ProtocolVersion));
        Connection.CurrentState = PacketState.Login;

        var received = null as IPacket;
        Task OnConnectionOnReceivedPacket(ReceivedPacketEventArgs args)
        {
            var pk = args.Packet;
            if (pk is ClientboundDisconnectPacket or ClientboundAckPacket)
            {
                received = pk;
            }
            return Task.CompletedTask;
        }
        Connection.ReceivedPacket += OnConnectionOnReceivedPacket;
        await Connection.SendPacketAsync(new ServerboundLoginPacket(Name));
        
        SpinWait.SpinUntil(() => received != null);
        Connection.ReceivedPacket -= OnConnectionOnReceivedPacket;
        
        if (received is ClientboundDisconnectPacket p)
        {
            throw new DisconnectedException(p);
        }
    }

    private async Task WaitForRoomCreationResponseAsync()
    {
        await Task.Yield();
        
        var received = null as IPacket;
        Task OnConnectionOnReceivedPacket(ReceivedPacketEventArgs args)
        {
            var pk = args.Packet;
            if (pk is ClientboundRoomDisconnectedPacket or ClientboundJoinedRoomPacket)
            {
                received = pk;
            }
            return Task.CompletedTask;
        }

        Connection.ReceivedPacket += OnConnectionOnReceivedPacket;

        SpinWait.SpinUntil(() => received != null);
        Connection.ReceivedPacket -= OnConnectionOnReceivedPacket;

        if (received is ClientboundRoomDisconnectedPacket p)
        {
            throw new RoomDisconnectedException(p);
        }
    }

    public async Task HostRoomAsync()
    {
        if (Session != null) return;
        await Connection.SendPacketAsync(new ServerboundHostRoomPacket());
        await WaitForRoomCreationResponseAsync();
        Logger.Verbose($"Hosted a room: #{Session?.RoomId}");
    }
    
    public async Task JoinRoomAsync(int roomId)
    {
        if (Session != null) return;
        await Connection.SendPacketAsync(new ServerboundJoinRoomPacket(roomId));
        await WaitForRoomCreationResponseAsync();
        Logger.Verbose($"Joined a room: #{Session?.RoomId}");
    }
}