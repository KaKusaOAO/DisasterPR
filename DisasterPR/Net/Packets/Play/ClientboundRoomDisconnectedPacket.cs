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
    public static ClientboundRoomDisconnectedPacket NoRoomLeft => new(RoomDisconnectReason.NoRoomLeft);
    public static ClientboundRoomDisconnectedPacket RoomPlaying => new(RoomDisconnectReason.RoomPlaying);
    public static ClientboundRoomDisconnectedPacket GuidDuplicate => new(RoomDisconnectReason.GuidDuplicate);

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

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRoomDisconnected(this);
}