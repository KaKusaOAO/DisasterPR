using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRoomDisconnectedPacket : IPacket<IClientPlayPacketHandler>
{
    public RoomDisconnectReason Reason { get; set; }
    public string? Message { get; set; }

    public ClientboundRoomDisconnectedPacket(RoomDisconnectReason reason)
    {
        Reason = reason;
    }

    public ClientboundRoomDisconnectedPacket(string message)
    {
        Reason = RoomDisconnectReason.Custom;
        Message = message;
    }

    public static ClientboundRoomDisconnectedPacket Custom(string message) => new(message);

    public static ClientboundRoomDisconnectedPacket NotFound => new(RoomDisconnectReason.NotFound);

    public static ClientboundRoomDisconnectedPacket RoomFull => new(RoomDisconnectReason.RoomFull);

    public ClientboundRoomDisconnectedPacket(MemoryStream stream)
    {
        Reason = (RoomDisconnectReason) stream.ReadVarInt();
        
        if (Reason == RoomDisconnectReason.Custom)
        {
            Message = stream.ReadUtf8String();
        }
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt((int) Reason);
        if (Reason == RoomDisconnectReason.Custom)
        {
            stream.WriteUtf8String(Message);
        }
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleRoomDisconnectedAsync(this);
}