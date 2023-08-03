using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundGameCurrentPlayerChangePacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    
    public ClientboundGameCurrentPlayerChangePacket(int index)
    {
        Index = index;
    }

    public ClientboundGameCurrentPlayerChangePacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Index = stream.ReadVarInt();
        }
        else
        {
            var payload = content.GetAsJsonObject();
            Index = payload["index"]!.GetValue<int>();
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleGameCurrentPlayerChange(this);
}