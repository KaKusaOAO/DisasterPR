using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using KaLib.Brigadier;
using KaLib.Brigadier.Builder;
using KaLib.Brigadier.Context;

namespace DisasterPR.Server.Commands;

public class AiCommand : IRegisteredCommand
{
    public static void Register(CommandDispatcher<IServerCommandSource> d)
    {
        d.Register(LiteralArgumentBuilder<IServerCommandSource>.Literal("ai").Executes(ExecuteAsync));
    }

    private static async Task ExecuteAsync(CommandContext<IServerCommandSource> context)
    {
        var source = context.GetSource();
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