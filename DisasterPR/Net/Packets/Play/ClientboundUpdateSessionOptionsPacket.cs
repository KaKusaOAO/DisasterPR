using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateSessionOptionsPacket : IPacket<IClientPlayPacketHandler> 
{
    public bool CanLockCards { get; set; }
    public int WinScore { get; set; }
    public CountdownTimeSet CountdownTimeSet { get; set; }
    public List<Guid> EnabledCategories { get; set; }
    
    public ClientboundUpdateSessionOptionsPacket(ISession session)
    {
        var options = session.Options;
        CanLockCards = options.CanLockCards;
        WinScore = options.WinScore;
        CountdownTimeSet = options.CountdownTimeSet;
        EnabledCategories = options.EnabledCategories.Select(c => c.Guid).ToList();
    }

    public ClientboundUpdateSessionOptionsPacket(BufferReader stream)
    {
        CanLockCards = stream.ReadBool();
        WinScore = stream.ReadVarInt();
        CountdownTimeSet = CountdownTimeSet.Deserialize(stream);
        EnabledCategories = stream.ReadList(s => s.ReadGuid());
    }

    public ClientboundUpdateSessionOptionsPacket(JsonObject payload)
    {
        CanLockCards = payload["canLockCards"]!.GetValue<bool>();
        WinScore = payload["winScore"]!.GetValue<int>();
        CountdownTimeSet = CountdownTimeSet.Deserialize(payload["timeSet"]!.AsObject());
        EnabledCategories = payload["categories"]!.AsArray(s => Guid.Parse(s!.GetValue<string>())).ToList();
    }

    public void Write(BufferWriter stream)
    {
        stream.WriteBool(CanLockCards);
        stream.WriteVarInt(WinScore);
        CountdownTimeSet.Serialize(stream);
        stream.WriteList(EnabledCategories, (s, g) => s.WriteGuid(g));
    }

    public void Write(JsonObject obj)
    {
        obj["canLockCards"] = CanLockCards;
        obj["winScore"] = WinScore;
        obj["timeSet"] = CountdownTimeSet.SerializeToJson();
        obj["categories"] = EnabledCategories.Select(s => JsonValue.Create(s.ToString())!).ToJsonArray();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateSessionOptions(this);
}