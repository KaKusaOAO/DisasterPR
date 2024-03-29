using DisasterPR.Cards;
using Mochi.Utils;

namespace DisasterPR.Sessions;

public abstract class Session<T> : ISession<T> where T : IPlayer
{
    public virtual CardPack? CardPack { get; set; }
    public SessionOptions Options { get; set; } = new();
    public int RoomId { get; set; }
    public List<T> Players { get; set; } = new();
    public T HostPlayer => Players.First();
    public abstract IGameState GameState { get; set; }
    public int RandomSeed { get; set; }

    private SemaphoreSlim _lock = new(1, 1);

    public Task AcquireAsync(Func<Task> func) => Common.AcquireSemaphoreAsync(_lock, func);

    protected Session()
    {
    }
}