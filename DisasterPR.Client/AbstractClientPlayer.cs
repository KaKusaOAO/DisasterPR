using DisasterPR.Cards;
using DisasterPR.Client.Sessions;
using DisasterPR.Sessions;

namespace DisasterPR.Client;

public abstract class AbstractClientPlayer : IPlayer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[]? AvatarData { get; set; }
    public string Identifier { get; set; }
    public LocalSession? Session { get; set;  }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public abstract List<HoldingWordCardEntry> HoldingCards { get; }
    public PlayerState State { get; set; }

    protected AbstractClientPlayer(string name)
    {
        Name = name;
    }
}