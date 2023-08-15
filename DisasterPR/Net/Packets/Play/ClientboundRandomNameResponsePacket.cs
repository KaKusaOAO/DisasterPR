using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRandomNameResponsePacket : IPacket<IClientPlayPacketHandler>, INoncePacket
{
    public Guid Nonce { get; set; }
    public string Name { get; set; }

    public ClientboundRandomNameResponsePacket(Guid nonce, string name)
    {
        Nonce = nonce;
        Name = name;
    }
    
    public ClientboundRandomNameResponsePacket(BufferReader stream)
    {
        Nonce = stream.ReadGuid();
        Name = stream.ReadUtf8String();
    }
    
    public ClientboundRandomNameResponsePacket(JsonObject payload)
    {
        Nonce = Guid.Parse(payload["nonce"]!.GetValue<string>());
        Name = payload["name"]!.GetValue<string>();
    }

    public void Write(BufferWriter writer)
    {
        writer.WriteGuid(Nonce);
        writer.WriteUtf8String(Name);
    }

    public void Write(JsonObject obj)
    {
        obj["nonce"] = Nonce.ToString();
        obj["name"] = Name;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRandomNameResponse(this);
}