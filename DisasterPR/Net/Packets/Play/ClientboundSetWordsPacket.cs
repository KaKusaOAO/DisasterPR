using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWordsPacket : IPacket<IClientPlayPacketHandler>
{
    public class Entry
    {
        public bool IsLocked { get; set; }
        public int Index { get; set; }

        public static Entry Deserialize(BufferReader stream)
        {
            return new Entry
            {
                IsLocked = stream.ReadBool(),
                Index = stream.ReadVarInt()
            };
        }

        public static Entry Deserialize(JsonNode? node)
        {
            var obj = (node as JsonObject)!;
            return new Entry
            {
                IsLocked = obj["locked"]!.GetValue<bool>(),
                Index = obj["index"]!.GetValue<int>()
            };
        }

        public void Serialize(BufferWriter stream)
        {
            stream.WriteBool(IsLocked);
            stream.WriteVarInt(Index);
        }

        public JsonObject SerializeToJson()
        {
            return new JsonObject
            {
                ["locked"] = IsLocked,
                ["index"] = Index
            };
        }
    }
    
    public List<Entry> Entries { get; set; }

    public ClientboundSetWordsPacket(List<Entry> entries)
    {
        Entries = entries;
    }

    public ClientboundSetWordsPacket(BufferReader stream)
    {
        Entries = stream.ReadList(Entry.Deserialize);
    }

    public ClientboundSetWordsPacket(JsonObject payload)
    {
        Entries = payload["entries"]!.AsArray(Entry.Deserialize).ToList();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteList(Entries, (s, i) => i.Serialize(s));
    }

    public void Write(JsonObject obj)
    {
        obj["entries"] = Entries.Select(e => e.SerializeToJson()).ToJsonArray();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetWords(this);
}