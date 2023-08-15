using DisasterPR.Cards;

namespace DisasterPR.Sessions;

public interface ISession
{
    public CardPack CardPack { get; set; }
    public SessionOptions Options { get; set; }
    public int RoomId { get; set; }
    public List<IPlayer> Players { get; set; }
    public IGameState GameState { get; set; }
    public int RandomSeed { get; set; }
}

public interface ISession<T> : ISession where T : IPlayer
{
    public new List<T> Players { get; set; }
    
    List<IPlayer> ISession.Players
    {
        get => Players.Select(p => (IPlayer)p).ToList();
        set => Players = value.Select(p => (T)p).ToList();
    }
}