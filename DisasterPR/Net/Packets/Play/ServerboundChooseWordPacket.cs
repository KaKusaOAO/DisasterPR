using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChooseWordPacket : IPacket<IServerPlayPacketHandler>
{
    public HashSet<int> Indices { get; set; }

    public ServerboundChooseWordPacket(HashSet<int> indices)
    {
        Indices = indices;
    }

    public ServerboundChooseWordPacket(BufferReader stream)
    {
        Indices = stream.ReadList(s => s.ReadVarInt()).ToHashSet();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteList(Indices.ToList(), (s, i) => s.WriteVarInt(i));
    }

    public void Write(JsonObject obj)
    {
        obj["indices"] = Indices.Select(i => JsonValue.Create(i)).ToJsonArray();
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleChooseWord(this);
}