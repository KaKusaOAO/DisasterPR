using DisasterPR.Extensions;
using DisasterPR.Sessions;

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

    public ClientboundUpdateSessionOptionsPacket(MemoryStream stream)
    {
        CanLockCards = stream.ReadBool();
        WinScore = stream.ReadVarInt();
        CountdownTimeSet = CountdownTimeSet.Deserialize(stream);
        EnabledCategories = stream.ReadList(s => s.ReadGuid());
    }

    public void Write(MemoryStream stream)
    {
        stream.WriteBool(CanLockCards);
        stream.WriteVarInt(WinScore);
        CountdownTimeSet.Serialize(stream);
        stream.WriteList(EnabledCategories, (s, g) => s.WriteGuid(g));
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateSessionOptions(this);
}