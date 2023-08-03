using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

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

    public ClientboundRoomDisconnectedPacket(BufferReader reader)
    {
        Reason = (RoomDisconnectReason) reader.ReadVarInt();
        
        if (Reason == RoomDisconnectReason.Custom)
        {
            Message = reader.ReadUtf8String();
        }
    }

    public ClientboundRoomDisconnectedPacket(JsonObject obj)
    {
        Reason = (RoomDisconnectReason) obj["reasonCode"]!.GetValue<int>();
        if (Reason == RoomDisconnectReason.Custom)
        {
            Message = obj["message"]!.GetValue<string>();
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt((int) Reason);
        if (Reason == RoomDisconnectReason.Custom)
        {
            stream.WriteUtf8String(Message!);
        }
    }

    public void Write(JsonObject obj)
    {
        obj["reasonCode"] = (int) Reason;
        if (Reason == RoomDisconnectReason.Custom)
        {
            obj["message"] = Message;
        }
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRoomDisconnected(this);
}