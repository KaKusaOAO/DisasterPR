using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Exceptions;

public class RoomDisconnectedException : Exception
{
    public RoomDisconnectReason Reason { get; set; }

    public RoomDisconnectedException(RoomDisconnectReason reason, string? message = null) : base(message)
    {
        Reason = reason;
    }

    public RoomDisconnectedException(ClientboundRoomDisconnectedPacket packet) : this(packet.Reason, packet.Message)
    {
        
    }
}