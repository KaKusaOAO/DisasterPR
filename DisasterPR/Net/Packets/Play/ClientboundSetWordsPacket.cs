﻿using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWordsPacket : IPacket<IClientPlayPacketHandler>
{
    public class Entry
    {
        public bool IsLocked { get; set; }
        public int Index { get; set; }

        public static Entry Deserialize(Stream stream)
        {
            return new Entry
            {
                IsLocked = stream.ReadBool(),
                Index = stream.ReadVarInt()
            };
        }

        public void Serialize(Stream stream)
        {
            stream.WriteBool(IsLocked);
            stream.WriteVarInt(Index);
        }
    }
    
    public List<Entry> Entries { get; set; }

    public ClientboundSetWordsPacket(List<Entry> entries)
    {
        Entries = entries;
    }

    public ClientboundSetWordsPacket(MemoryStream stream)
    {
        Entries = stream.ReadList(Entry.Deserialize);
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteList(Entries, (s, i) => i.Serialize(s));
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetWords(this);
}