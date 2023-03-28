using DisasterPR.Extensions;
using KaLib.Utils;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSystemChatPacket : IPacket<IClientPlayPacketHandler>
{
    public LogLevel Level { get; set; }
    public string Content { get; set; }

    public ClientboundSystemChatPacket(string content, LogLevel level = LogLevel.Info)
    {
        Level = level;
        Content = content;
    }
    
    public ClientboundSystemChatPacket(MemoryStream stream)
    {
        Level = stream.ReadEnum<LogLevel>();
        Content = stream.ReadUtf8String();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteEnum(Level);
        stream.WriteUtf8String(Content);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSystemChat(this);
}