using DisasterPR.Net.Packets;

namespace DisasterPR.Client;

public class RemotePlayer : AbstractClientPlayer
{
    public override List<HoldingWordCardEntry> HoldingCards => throw new NotSupportedException();
    
    public RemotePlayer(Guid guid, string name) : base(name)
    {
        Id = guid;
    }

    public RemotePlayer(AddPlayerEntry entry) : this(entry.Guid, entry.Name)
    {
        
    }
}