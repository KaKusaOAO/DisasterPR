using DisasterPR.Cards.Providers;
using DisasterPR.Server.Commands.Arguments;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Extensions;
using DisasterPR.Sessions;
using KaLib.Brigadier;
using KaLib.Brigadier.Context;
using KaLib.Nbt;
using KaLib.Texts;
using KaLib.Utils;
using SessionOptions = DisasterPR.Sessions.SessionOptions;

namespace DisasterPR.Server.Commands;

public class SessionCommand : Command, IRegisteredCommand
{
    public static void Register(CommandDispatcher<CommandSource> d)
    {
        d.Register(Literal("session")
            .Requires(s => s.IsCapableOfSessionHostOperations())
            // -> /session dump
            .Then(Literal("dump")
                .Requires(s => s.Sender is ConsoleCommandSender)
                .Executes(c => ExecuteDumpAsync(c, false))
                .Then(Argument("path", NbtPathArgument.Path())
                    .Executes(c => ExecuteDumpAsync(c, true))
                )
            )
            // -> /session cardpack ...
            .Then(Literal("cardpack")
                // -> /session cardpack reload
                .Then(Literal("reload")
                    .Executes(ExecuteReloadAsync)
                )
            )
            // -> /session options ...
            .Then(Literal("options")
                // -> /session options reset
                .Then(Literal("reset")
                    .Executes(ExecuteResetOptionsAsync)
                )
            )
            .Then(Literal("disband")
                .Executes(ExecuteDisbandAsync)
            )
        );
    }

    private static async Task ExecuteDumpAsync(CommandContext<CommandSource> context, bool hasPath)
    {
        if (!await CheckSourceInSessionAsync(context)) return;
        var session = context.GetSource().Session!;

        var nbt = session.CreateSnapshot() as NbtTag;
        if (hasPath)
        {
            try
            {
                var path = context.GetArgument<NbtPath>("path");
                nbt = path.Navigate(nbt);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return;
            }
        }
        
        Logger.Info(TranslateText.Of("Data of %s is: ")
            .AddWith(LiteralText.Of(hasPath ? context.GetArgument<NbtPath>("path").ToPathString() : "$")
                .SetColor(TextColor.Aqua))
            .AddExtra(nbt.ToText()));
    }

    private static async Task ExecuteDisbandAsync(CommandContext<CommandSource> context)
    {
        if (!await CheckSourceInSessionAsync(context)) return;

        var source = context.GetSource();
        var session = source.Session!;
        var state = session.GameState.CurrentState;
        if (state != StateOfGame.Waiting)
        {
            await source.SendErrorMessageAsync("不能在這個時候解散房間。");
            return;
        }

        foreach (var player in session.Players.ToList())
        {
            await player.KickFromSessionAsync("房間已被解散。");
            await session.PlayerLeaveAsync(player);
        }
        await source.SendMessageAsync($"已解散房間 #{session.RoomId}。");
    }

    private static async Task ExecuteResetOptionsAsync(CommandContext<CommandSource> context)
    {
        if (!await CheckSourceInSessionAsync(context)) return;

        var source = context.GetSource();
        var session = source.Session!;
        var state = session.GameState.CurrentState;
        if (state != StateOfGame.Waiting)
        {
            await source.SendErrorMessageAsync("現在不能重設房間設定。");
            return;
        }

        session.Options = new SessionOptions();
        session.Options.EnabledCategories.Add(session.CardPack!.Categories.First());

        foreach (var player in session.Players)
        {
            if (player != source.Sender)
            {
                await player.SendToastAsync($"{source.Sender.Name}已重設房間設定。");
            }

            await player.UpdateSessionOptions(session);
        }
        
        await source.SendMessageAsync($"已成功為房間 #{session.RoomId} 重設房間設定。");
    }

    private static async Task ExecuteReloadAsync(CommandContext<CommandSource> context)
    {
        if (!await CheckSourceInSessionAsync(context)) return;
        
        var source = context.GetSource();
        var session = source.Session!;
        var state = session.GameState.CurrentState;
        if (state != StateOfGame.Waiting)
        {
            await source.SendErrorMessageAsync("現在不能重新載入卡包。");
            return;
        }

        var builder = IPackProvider.Default.MakeBuilder();
        var pack = builder.Build();
        await session.SetAndUpdateCardPackAsync(pack);
        
        foreach (var player in session.Players.Where(player => player != source.Sender))
        {
            await player.SendToastAsync($"{source.Sender.Name}已為房間重新載入卡包。");
        }
        await source.SendMessageAsync($"已成功為房間 #{session.RoomId} 重新載入卡包。");
    }
}