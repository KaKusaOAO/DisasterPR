using DisasterPR.Net.Packets.Login;

namespace DisasterPR.Exceptions;

public class DisconnectedException : Exception
{
    public PlayerKickReason Reason { get; set; }

    public DisconnectedException(PlayerKickReason reason, string? message = null) : base(message)
    {
        Reason = reason;
    }

    public DisconnectedException(ClientboundDisconnectPacket packet) : this(packet.Reason, packet.Message)
    {
        
    }
}