namespace DisasterPR.Client;

public abstract class AbstractClientPlayer : IPlayer
{
    public Guid Id { get; set; }
    public string Name { get; }
    public LocalSession? Session { get; set;  }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }

    protected AbstractClientPlayer(string name)
    {
        Name = name;
    }
}