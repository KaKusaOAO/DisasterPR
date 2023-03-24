using DisasterPR.Cards.Providers;
using DisasterPR.Server.Commands.Senders;
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
            await source.SendErrorMessageAsync("You are not in a session.");
            return;
        }

        var builder = IPackProvider.Default.MakeBuilder();
        var pack = builder.Build();
        await session.SetAndUpdateCardPackAsync(pack);
        await source.SendMessageAsync($"The card pack has been reset for session #{session.RoomId}");
    }
}