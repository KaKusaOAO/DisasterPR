using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundDisconnectPacket : IPacket<IClientLoginPacketHandler>
{
    public PlayerKickReason Reason { get; set; }
    public string? Message { get; set; }

    public ClientboundDisconnectPacket(string message)
    {
        Reason = PlayerKickReason.Custom;
        Message = message;
    }

    public ClientboundDisconnectPacket(PlayerKickReason reason)
    {
        Reason = reason;
    }

    public ClientboundDisconnectPacket(MemoryStream stream)
    {
        Reason = (PlayerKickReason)stream.ReadVarInt();
        if (Reason == PlayerKickReason.Custom)
        {
            Message = stream.ReadUtf8String();
        }
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt((int)Reason);
        if (Reason == PlayerKickReason.Custom)
        {
            stream.WriteUtf8String(Message);
        }
    }

    public Task HandleAsync(IClientLoginPacketHandler handler) => handler.HandleDisconnectAsync(this);
}