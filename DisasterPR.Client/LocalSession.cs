namespace DisasterPR.Client;

public class LocalSession : ISession<AbstractClientPlayer>
{
    public SessionOptions Options { get; set; } = new();
    public int RoomId { get; set; }
    public List<AbstractClientPlayer> Players { get; set; } = new();
}