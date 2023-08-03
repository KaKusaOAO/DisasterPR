using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckLoginPacket : IPacket<IClientLoginPacketHandler>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public ClientboundAckLoginPacket(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public ClientboundAckLoginPacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Id = stream.ReadGuid();
            Name = stream.ReadUtf8String();
        }
        else // if (content.Type == PacketContentType.Json)
        {
            var obj = content.GetAsJsonObject();
            Id = Guid.Parse(obj["id"]!.GetValue<string>());
            Name = obj["name"]!.GetValue<string>();
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
        stream.WriteUtf8String(Name);
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = Id.ToString();
        obj["name"] = Name;
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleAckLogin(this);
}