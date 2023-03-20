namespace DisasterPR.Server;

public class ServerPlayer : IPlayer
{
    public string Name { get; set; }
    
    public ISession? Session { get; }
    public int Score { get; set; }

    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }
}