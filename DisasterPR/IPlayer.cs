namespace DisasterPR;

public interface IPlayer
{
    public string Name { get; }
    public ISession? Session { get; }
    public int Score { get; set; }
}