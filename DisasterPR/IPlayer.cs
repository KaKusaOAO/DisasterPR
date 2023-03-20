namespace DisasterPR;

public interface IPlayer
{
    public Guid Id { get; }
    public string Name { get; }
    public ISession? Session { get; }
    public int Score { get; set; }
}