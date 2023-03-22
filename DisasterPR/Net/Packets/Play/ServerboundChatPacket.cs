using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChatPacket : IPacket<IServerPlayPacketHandler>
{
    public string Content { get; set; }

    public ServerboundChatPacket(string content)
    {
        Content = content;
    }
    
    public ServerboundChatPacket(MemoryStream stream)
    {
        Content = stream.ReadUtf8String();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteUtf8String(Content);
    }

    public Task HandleAsync(IServerPlayPacketHandler handler) => handler.HandleChatAsync(this);
}