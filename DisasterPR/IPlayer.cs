using DisasterPR.Sessions;

namespace DisasterPR;

public interface IPlayer
{
    public Guid Id { get; }
    public string Name { get; }
    public byte[]? AvatarData { get; }
    public string Identifier { get; }
    public ISession? Session { get; }
    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; }
    public PlayerState State { get; set; }
}