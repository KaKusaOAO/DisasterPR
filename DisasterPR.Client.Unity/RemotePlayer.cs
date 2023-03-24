using DisasterPR.Net.Packets;

namespace DisasterPR.Client.Unity;

public class RemotePlayer : AbstractClientPlayer
{
    public RemotePlayer(Guid guid, string name) : base(name)
    {
        Id = guid;
    }

    public RemotePlayer(AddPlayerEntry entry) : this(entry.Guid, entry.Name)
    {
        
    }
}