using DisasterPR.Proxy.Sessions;

namespace DisasterPR.Proxy.Commands.Senders;

public class CommandSource
{
    public ICommandSender Sender { get; set; }
    public ServerSession? Session { get; set; }

    public CommandSource Copy()
    {
        return new CommandSource
        {
            Sender = Sender,
            Session = Session
        };
    }

    public CommandSource Modify(Action<CommandSource> modifier)
    {
        modifier(this);
        return this;
    }
    
    public static CommandSource OfConsole() => new()
    {
        Sender = new ConsoleCommandSender()
    };

    public static CommandSource OfPlayer(ServerPlayer player) => new()
    {
        Sender = player,
        Session = player.Session
    };
    
    public bool IsCapableOfSessionHostOperations()
    {
        return Sender is ConsoleCommandSender ||
               (Sender is ServerPlayer {Session: { }} p && p.Session.HostPlayer == p);
    }
    
    public Task SendMessageAsync(string content) => Sender.SendMessageAsync(content);
    public Task SendErrorMessageAsync(string content) => Sender.SendErrorMessageAsync(content);
}