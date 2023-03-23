using DisasterPR.Server.Sessions;

namespace DisasterPR.Server.Commands.Senders;

public class ServerCommandSource : IServerCommandSource
{
    public ICommandSender Sender { get; init; }
    public ServerSession? Session { get; init; }

    public static ServerCommandSource OfConsole() => new()
    {
        Sender = new ConsoleCommandSender()
    };

    public static ServerCommandSource OfPlayer(ServerPlayer player) => new()
    {
        Sender = player,
        Session = player.Session
    };
}