namespace DisasterPR.Events;

public delegate Task PlayerChatEventDelegate(PlayerChatEventArgs e);

public class PlayerChatEventArgs
{
    public string PlayerName { get; set; }
    public string Content { get; set; }
}