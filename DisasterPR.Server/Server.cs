namespace DisasterPR.Server;

public class Server
{
    private static Server _instance;

    public static Server Instance => _instance;

    public Random Random { get; } = new Random();

    public Server()
    {
        if (_instance != null)
        {
            throw new Exception("Server is already registered");
        }
        
        _instance = this;
    }
    
    public List<ServerPlayer> Players { get; } = new();

    public Dictionary<int, ServerSession> Sessions { get; } = new();
}