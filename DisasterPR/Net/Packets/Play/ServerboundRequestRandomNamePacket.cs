using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestRandomNamePacket : IPacket<IServerPlayPacketHandler>, INoncePacket
{
    public Guid Nonce { get; set; }

    public ServerboundRequestRandomNamePacket()
    {
        Nonce = Guid.NewGuid();
    }
    
    public ServerboundRequestRandomNamePacket(BufferReader stream)
    {
        Nonce = stream.ReadGuid();
    }
    
    public ServerboundRequestRandomNamePacket(JsonObject payload)
    {
        Nonce = Guid.Parse(payload["nonce"]!.GetValue<string>());
    }

    public void Write(BufferWriter writer)
    {
        writer.WriteGuid(Nonce);
    }

    public void Write(JsonObject obj)
    {
        obj["nonce"] = Nonce.ToString();
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestRandomName(this);
}