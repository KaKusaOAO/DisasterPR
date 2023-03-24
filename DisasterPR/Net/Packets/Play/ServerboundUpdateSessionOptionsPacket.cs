﻿using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundUpdateSessionOptionsPacket : IPacket<IServerPlayPacketHandler> 
{
    public int WinScore { get; set; }
    public CountdownTimeSet CountdownTimeSet { get; set; }
    public List<Guid> EnabledCategories { get; set; }
    
    public ServerboundUpdateSessionOptionsPacket(ISession session)
    {
        var options = session.Options;
        WinScore = options.WinScore;
        CountdownTimeSet = options.CountdownTimeSet;
        EnabledCategories = options.EnabledCategories.Select(c => c.Guid).ToList();
    }

    public ServerboundUpdateSessionOptionsPacket(int winScore, CountdownTimeSet timeSet, List<CardCategory> categories)
    {
        WinScore = winScore;
        CountdownTimeSet = timeSet;
        EnabledCategories = categories.Select(c => c.Guid).ToList();
    }

    public ServerboundUpdateSessionOptionsPacket(MemoryStream stream)
    {
        WinScore = stream.ReadVarInt();
        CountdownTimeSet = CountdownTimeSet.Deserialize(stream);
        EnabledCategories = stream.ReadList(s => s.ReadGuid());
    }

    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(WinScore);
        CountdownTimeSet.Serialize(stream);
        stream.WriteList(EnabledCategories, (s, g) => s.WriteGuid(g));
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleUpdateSessionOptions(this);
}