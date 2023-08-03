using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundRevealChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Guid { get; set; }

    public ClientboundRevealChosenWordEntryPacket(Guid guid)
    {
        Guid = guid;
    }

    public ClientboundRevealChosenWordEntryPacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Guid = stream.ReadGuid();
        }
        else
        {
            var payload = content.GetAsJsonObject();
            Guid = Guid.Parse(payload["id"]!.GetValue<string>());
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Guid);
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = Guid.ToString();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleRevealChosenWordEntry(this);
}