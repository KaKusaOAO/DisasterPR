using DisasterPR.Server.Commands.Senders;
using KaLib.Brigadier;
using KaLib.Brigadier.Context;
using KaLib.Utils;

namespace DisasterPR.Server.Commands;

public class HelpCommand : Command, IRegisteredCommand
{
    public static void Register(CommandDispatcher<IServerCommandSource> d)
    {
        d.Register(Literal("help").Executes(ExecuteAsync));
    }

    private static Task ExecuteAsync(CommandContext<IServerCommandSource> c)
    {
        var d = GameServer.Instance.Dispatcher;
        var source = c.GetSource();
        var usages = d.GetSmartUsage(d.GetRoot(), source);
        foreach (var str in usages.Values)
        {
            Logger.Info($"Usage: /{str}");
        }

        return Task.CompletedTask;
    }
}