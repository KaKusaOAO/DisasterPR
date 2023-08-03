using System.Text.Json.Nodes;
using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Id { get; set; }
    public Guid? PlayerId { get; set; }
    public List<int> Words { get; set; }

    public ClientboundAddChosenWordEntryPacket(Guid id, Guid? playerId, List<int> words)
    {
        Id = id;
        PlayerId = playerId;
        Words = words;
    }

    public ClientboundAddChosenWordEntryPacket(BufferReader stream)
    {
        Id = stream.ReadGuid();
        if (stream.ReadBool())
        {
            PlayerId = stream.ReadGuid();
        }

        Words = stream.ReadList(s => s.ReadVarInt());
    }
    
    public ClientboundAddChosenWordEntryPacket(JsonObject payload)
    {
        Id = Guid.Parse(payload["id"]!.GetValue<string>());
        if (payload.TryGetPropertyValue("playerId", out var value))
        {
            PlayerId = Guid.Parse(value!.GetValue<string>());
        }

        Words = payload["words"]!.AsArray().Select(v => v!.GetValue<int>()).ToList();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
        stream.WriteBool(PlayerId.HasValue);
        if (PlayerId.HasValue)
        {
            stream.WriteGuid(PlayerId.Value);
        }

        stream.WriteList(Words, (s, i) => s.WriteVarInt(i));
    }

    public void Write(JsonObject obj)
    {
        obj["id"] = Id.ToString();
        if (PlayerId.HasValue)
        {
            obj["playerId"] = PlayerId.Value;
        }

        var arr = new JsonArray();
        foreach (var word in Words) arr.Add(word);
        obj["words"] = arr;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleAddChosenWordEntry(this);
}