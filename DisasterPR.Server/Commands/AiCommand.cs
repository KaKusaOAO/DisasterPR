using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using Mochi.Brigadier;
using Mochi.Brigadier.Builder;
using Mochi.Brigadier.Context;

namespace DisasterPR.Server.Commands;

public class AiCommand : Command, IRegisteredCommand
{
    public static void Register(CommandDispatcher<CommandSource> d)
    {
        d.Register(LiteralArgumentBuilder<CommandSource>.Literal("ai")
            .Requires(s => s.IsCapableOfSessionHostOperations())
            .Executes(ExecuteAsync)
        );
    }

    private static async Task ExecuteAsync(CommandContext<CommandSource> context)
    {
        var source = context.Source;
        var session = source.Session;
        if (session == null)
        {
            await source.SendErrorMessageAsync("你不在房間內。");
            return;
        }

        var count = session.Players.Count;
        for (var i = count; i < Constants.SessionMaxPlayers; i++)
        {
            var ai = new AIPlayer();
            await session.PlayerJoinAsync(ai);

            ai.State = PlayerState.Ready;
            foreach (var p in session.Players)
            {
                await p.OnOtherPlayerUpdateStateAsync(ai);
            }
        }

        await source.SendMessageAsync("已加滿 AI 玩家。");
    }
}