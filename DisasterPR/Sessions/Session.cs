using DisasterPR.Cards;
using KaLib.Utils;

namespace DisasterPR.Sessions;

public abstract class Session<T> : ISession<T> where T : IPlayer
{
    public CardPack CardPack { get; set; } = CardPack.GetUpstreamAsync().Result;
    public SessionOptions Options { get; set; } = new();
    public int RoomId { get; set; }
    public List<T> Players { get; set; } = new();
    public T HostPlayer => Players.First();
    public abstract IGameState GameState { get; set; }
    
    private SemaphoreSlim _lock = new(1, 1);

    public Task AcquireAsync(Func<Task> func) => Common.AcquireSemaphoreAsync(_lock, func);

    protected Session()
    {
        Options.EnabledCategories.Add(CardPack.Categories.First());
    }
}