using DisasterPR.Server.Sessions;

namespace DisasterPR.Server;

public class GameServer
{
    private static GameServer _instance;

    public static GameServer Instance => _instance;

    public Random Random { get; } = new Random();

    public GameServer()
    {
        if (_instance != null)
        {
            throw new Exception("Server is already registered");
        }
        
        _instance = this;
        
        _ = Bootstrap.BootAsync();
    }
    
    public List<ServerPlayer> Players { get; } = new();

    public Dictionary<int, ServerSession> Sessions { get; } = new();
}