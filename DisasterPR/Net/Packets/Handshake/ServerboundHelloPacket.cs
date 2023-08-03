using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Handshake;

public class ServerboundHelloPacket : IPacket<IServerHandshakePacketHandler>
{
    public int Version { get; set; }

    public ServerboundHelloPacket(int version)
    {
        Version = version;
    }
    
    public ServerboundHelloPacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Version = stream.ReadVarInt();    
        } else // if (content.Type == PacketContentType.Json)
        {
            var payload = content.GetAsJsonObject();
            Version = payload["version"]!.GetValue<int>();
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Version);
    }

    public void Write(JsonObject obj)
    {
        obj["version"] = Version;
    }

    public void Handle(IServerHandshakePacketHandler handler) => handler.HandleHello(this);
}