using DisasterPR.Net.Packets;

namespace DisasterPR.Client;

public class RemotePlayer : AbstractClientPlayer
{
    public override List<HoldingWordCardEntry> HoldingCards => throw new NotSupportedException();

    public RemotePlayer(PlayerDataModel model) : base(model)
    {
        
    }
}