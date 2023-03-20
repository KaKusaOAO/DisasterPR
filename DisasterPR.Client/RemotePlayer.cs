namespace DisasterPR.Client;

public class RemotePlayer : AbstractClientPlayer
{
    public string Name { get; }
    public ISession? Session { get; }
    public int Score { get; set; }

    public RemotePlayer(string name) : base(name)
    {
        
    }
}