using KaLib.Utils;

namespace DisasterPR;

public interface ISession
{
    public SessionOptions Options { get; set; }
    public int RoomId { get; set; }
    public List<IPlayer> Players { get; set; }
}

public interface ISession<T> : ISession where T : IPlayer
{
    public List<T> Players { get; set; }

    List<IPlayer> ISession.Players
    {
        get => Players.Select(p => (IPlayer)p).ToList();
        set => Players = value.Select(p => (T)p).ToList();
    }
}

public class Session<T> : ISession<T> where T : IPlayer
{
    public SessionOptions Options { get; set; } = new();
    public int RoomId { get; set; }
    public List<T> Players { get; set; } = new();
    private SemaphoreSlim _lock = new(1, 1);

    public Task AcquireAsync(Func<Task> func) => Common.AcquireSemaphoreAsync(_lock, func);
}