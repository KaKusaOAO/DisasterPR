using DisasterPR.Cards;
using DisasterPR.Client.Sessions;
using DisasterPR.Sessions;

namespace DisasterPR.Client;

public abstract class AbstractClientPlayer : IPlayer
{
    public Guid Id { get; set; }
    public string Name { get; }
    public LocalSession? Session { get; set;  }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public List<WordCard> HoldingCards { get; } = new();

    protected AbstractClientPlayer(string name)
    {
        Name = name;
    }
}