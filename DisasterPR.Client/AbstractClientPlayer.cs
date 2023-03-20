namespace DisasterPR.Client;

public abstract class AbstractClientPlayer : IPlayer
{
    public string Name { get; }
    public ISession? Session { get; set;  }
    public int Score { get; set; }

    protected AbstractClientPlayer(string name)
    {
        Name = name;
    }
}