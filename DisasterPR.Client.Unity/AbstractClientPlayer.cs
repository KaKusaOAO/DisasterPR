using DisasterPR.Client.Unity.Sessions;
using DisasterPR.Sessions;

namespace DisasterPR.Client.Unity;

public abstract class AbstractClientPlayer : IPlayer
{
    public Guid Id { get; set; }
    public string Name { get; }
    public LocalSession? Session { get; set;  }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; } = new();
    public PlayerState State { get; set; }

    protected AbstractClientPlayer(string name)
    {
        Name = name;
    }
}