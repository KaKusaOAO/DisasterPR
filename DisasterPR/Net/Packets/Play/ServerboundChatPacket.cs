using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChatPacket : IPacket<IServerPlayPacketHandler>
{
    public string Content { get; set; }

    public ServerboundChatPacket(string content)
    {
        Content = content;
    }
    
    public ServerboundChatPacket(BufferReader stream)
    {
        Content = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteUtf8String(Content);
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleChat(this);
}