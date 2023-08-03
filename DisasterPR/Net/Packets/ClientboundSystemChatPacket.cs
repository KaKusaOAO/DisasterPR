using System.Text.Json.Nodes;
using Mochi.IO;
using Mochi.Utils;

namespace DisasterPR.Net.Packets;

public class ClientboundSystemChatPacket : IPacket<IClientSystemChatHandler>
{
    public LogLevel Level { get; set; }
    public string Content { get; set; }

    public ClientboundSystemChatPacket(string content, LogLevel level = LogLevel.Info)
    {
        Level = level;
        Content = content;
    }
    
    public ClientboundSystemChatPacket(BufferReader stream)
    {
        Level = stream.ReadEnum<LogLevel>();
        Content = stream.ReadUtf8String();
    }

    public ClientboundSystemChatPacket(JsonObject payload)
    {
        Level = (LogLevel) payload["level"]!.GetValue<int>();
        Content = payload["content"]!.GetValue<string>();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteEnum(Level);
        stream.WriteUtf8String(Content);
    }

    public void Write(JsonObject obj)
    {
        obj["level"] = (int) Level;
        obj["content"] = Content;
    }

    public void Handle(IClientSystemChatHandler handler) => handler.HandleSystemChat(this);
}