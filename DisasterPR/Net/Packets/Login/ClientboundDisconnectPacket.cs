using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

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
        if (reason == PlayerKickReason.Custom)
        {
            throw new ArgumentException(
                "If you want to send custom reason messages, use new ClientboundDisconnectPacket(string).");
        }
        
        Reason = reason;
    }

    public ClientboundDisconnectPacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Reason = (PlayerKickReason) stream.ReadVarInt();
            if (Reason == PlayerKickReason.Custom)
            {
                Message = stream.ReadUtf8String();
            }
        }
        else
        {
            var obj = content.GetAsJsonObject();
            Reason = (PlayerKickReason) obj["reasonCode"]!.GetValue<int>();
            if (Reason == PlayerKickReason.Custom)
            {
                Message = obj["message"]!.GetValue<string>();
            }
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt((int)Reason);
        if (Reason == PlayerKickReason.Custom)
        {
            stream.WriteUtf8String(Message!);
        }
    }

    public void Write(JsonObject obj)
    {
        obj["reasonCode"] = (int) Reason;
        if (Reason == PlayerKickReason.Custom)
        {
            obj["message"] = Message;
        }
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleDisconnect(this);
}