using System.Text;
using DisasterPR.Server.Commands.Senders;
using Mochi.Brigadier;
using Mochi.Brigadier.Context;

namespace DisasterPR.Server.Commands;

public class HelpCommand : Command, IRegisteredCommand
{
    public static void Register(CommandDispatcher<CommandSource> d)
    {
        d.Register(Literal("help").Executes(ExecuteAsync));
    }

    private static async Task ExecuteAsync(CommandContext<CommandSource> c)
    {
        var d = GameServer.Instance.Dispatcher;
        var source = c.Source;
        var usages = d.GetSmartUsage(d.Root, source);
        var sb = new StringBuilder();
        foreach (var str in usages.Values)
        {
            sb.AppendLine($"用法：/{str}");
        }
        
        await source.SendMessageAsync(sb.ToString());
    }
}