using DisasterPR.Server.Commands.Senders;
using KaLib.Brigadier;
using KaLib.Brigadier.Arguments;
using KaLib.Brigadier.Builder;
using KaLib.Brigadier.Context;
using KaLib.Texts;
using KaLib.Utils;
using KaLib.Utils.Extensions;

namespace DisasterPR.Server.Commands;

public class Command
{
    public static LiteralArgumentBuilder<IServerCommandSource> Literal(string name)
    {
        return LiteralArgumentBuilder<IServerCommandSource>.Literal(name);
    }

    public static RequiredArgumentBuilder<IServerCommandSource, T> Argument<T>(string name, IArgumentType<T> type)
    {
        return RequiredArgumentBuilder<IServerCommandSource, T>.Argument(name, type);
    }

    public static Task ExecuteCommandAsync(ServerPlayer player, string input) =>
        ExecuteCommandAsync(IServerCommandSource.OfPlayer(player), Constants.CommandPrefix, input);

    public static Task ExecuteCommandByConsoleAsync(string input) =>
        ExecuteCommandAsync(IServerCommandSource.OfConsole(), "", input);

    public static async Task ExecuteCommandAsync(IServerCommandSource source, string prefix, string input)
    {
        var dispatcher = GameServer.Instance.Dispatcher;
        var cursorOffset = prefix.Length;
        var result = dispatcher.Parse(input[cursorOffset..], source);
        await ExecuteParseResultAsync(result, prefix, input, cursorOffset);
    }
    
    private static async Task ExecuteParseResultAsync(ParseResults<IServerCommandSource> result, string prefix, 
        string input, int cursorOffset)
    {
        var dispatcher = GameServer.Instance.Dispatcher;
        var source = result.GetContext().GetSource();
        var sender = source.Sender;

        var exceptions = result.GetExceptions().Values;
        if (exceptions.Any())
        {
            var err = exceptions.First();
            var errMessage = err.GetRawMessage();
            var messageString = errMessage.GetString();

            var cursor = err.GetCursor() + cursorOffset;
            if (messageString != null)
            {
                await sender.SendErrorMessageAsync($"{input[..cursor]} <- 這裡\n{messageString}");
            }
            
            return;
        }

        var reader = result.GetReader();
        if (reader.CanRead())
        {
            var read = reader.GetRead().Length;
            if (read == 0)
            {
                await sender.SendErrorMessageAsync($"未知的指令: {input}");
                return;
            }

            async Task Run()
            {
                var context = result.GetContext().GetLastChild();
                var usage = GetUsageText(dispatcher!, context, input, cursorOffset);
                if (!string.IsNullOrEmpty(usage))
                {
                    await sender!.SendErrorMessageAsync($"用法: {usage}");
                }
            }

            await Run();
            return;
        }

        var context = result.GetContext().GetLastChild();
        if (context.GetCommand() == null)
        {
            var ctx = context.GetLastChild();
            var usage = GetUsageText(dispatcher, ctx, input, cursorOffset);
            if (!string.IsNullOrEmpty(usage))
            {
                await sender.SendErrorMessageAsync($"用法: {usage}");
            }
            return;
        }

        Logger.Info(
            TranslateText.Of("%s issued a command: %s at %s")
                .AddWith(Texts.OfSender(sender))
                .AddWith(LiteralText.Of(input).SetColor(TextColor.Aqua))
                .AddWith(Texts.OfSession(source.Session))
        );
        await dispatcher.ExecuteAsync(result);
    }
    
    private static string GetUsageText(CommandDispatcher<IServerCommandSource> dispatcher,
        CommandContextBuilder<IServerCommandSource> context, string input, int cursorOffset)
    {
        var nodes = context.GetNodes();
        if (!nodes.Any()) return "";
        
        var node = nodes[^1];
        var parent = nodes.Count > 1 ? nodes[^2] : null;
        cursorOffset += node.GetRange().GetStart();
        var usage = dispatcher.GetSmartUsage(parent?.GetNode() ?? dispatcher.GetRoot(), context.GetSource());
        return input[..cursorOffset] + usage[node.GetNode()];
    }
}