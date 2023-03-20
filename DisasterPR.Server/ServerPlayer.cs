namespace DisasterPR.Server;

public class ServerPlayer : IPlayer
{
    public Guid Id { get; }
    public string Name { get; set; }
    
    public ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }

    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer(ServerToPlayerConnection connection)
    {
        Id = Guid.NewGuid();
        Connection = connection;
    }
}