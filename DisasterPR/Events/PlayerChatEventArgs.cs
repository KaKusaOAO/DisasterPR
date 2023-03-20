namespace DisasterPR.Events;

public delegate Task PlayerChatEventDelegate(PlayerChatEventArgs e);

public class PlayerChatEventArgs
{
    public IPlayer Player { get; set; }
    public string Content { get; set; }
}