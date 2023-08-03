using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundChatPacket : IPacket<IClientPlayPacketHandler>
{
    public string Player { get; set; }
    public string Content { get; set; }

    public ClientboundChatPacket(string player, string content)
    {
        Player = player;
        Content = content;
    }
    
    public ClientboundChatPacket(BufferReader stream)
    {
        Player = stream.ReadUtf8String();
        Content = stream.ReadUtf8String();
    }
    
    public ClientboundChatPacket(JsonObject payload)
    {
        Player = payload["playerName"]!.GetValue<string>();
        Content = payload["content"]!.GetValue<string>();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteUtf8String(Player);
        stream.WriteUtf8String(Content);
    }

    public void Write(JsonObject obj)
    {
        obj["playerName"] = Player;
        obj["content"] = Content;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleChat(this);
}