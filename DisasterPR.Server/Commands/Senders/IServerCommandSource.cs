using DisasterPR.Server.Sessions;

namespace DisasterPR.Server.Commands.Senders;

public interface IServerCommandSource
{
    public ICommandSender Sender { get; }
    public ServerSession? Session { get; }

    public static IServerCommandSource OfConsole() => new ServerCommandSource()
    {
        Sender = new ConsoleCommandSender()
    };

    public static IServerCommandSource OfPlayer(ServerPlayer player) => new ServerPlayerCommandSource()
    {
        Player = player
    };
}