using DisasterPR.Cards.Providers;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Sessions;
using KaLib.Brigadier;
using KaLib.Brigadier.Context;

namespace DisasterPR.Server.Commands;

public class CardPackCommand : Command, IRegisteredCommand
{
    public static void Register(CommandDispatcher<IServerCommandSource> d)
    {
        d.Register(Literal("cardpack")
            .Then(Literal("reset")
                .Executes(ExecuteResetAsync)
            )
        );
    }

    private static async Task ExecuteResetAsync(CommandContext<IServerCommandSource> context)
    {
        var source = context.GetSource();
        var session = source.Session;
        if (session == null)
        {
            await source.SendErrorMessageAsync("你不在房間內。");
            return;
        }

        var state = session.GameState.CurrentState;
        if (state != StateOfGame.Waiting)
        {
            await source.SendErrorMessageAsync("現在不能重新載入卡包。");
            return;
        }

        var builder = IPackProvider.Default.MakeBuilder();
        var pack = builder.Build();
        await session.SetAndUpdateCardPackAsync(pack);
        await source.SendMessageAsync($"已成功為房間 #{session.RoomId} 重新載入卡包。");
    }
}