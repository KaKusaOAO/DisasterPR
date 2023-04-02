using DisasterPR.Server.Commands.Senders;
using Mochi.Brigadier;
using Mochi.Brigadier.Arguments;
using Mochi.Brigadier.Builder;
using Mochi.Brigadier.Context;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Server.Commands;

public class Command
{
    public static LiteralArgumentBuilder<CommandSource> Literal(string name)
    {
        return LiteralArgumentBuilder<CommandSource>.Literal(name);
    }

    public static RequiredArgumentBuilder<CommandSource, T> Argument<T>(string name, IArgumentType<T> type)
    {
        return RequiredArgumentBuilder<CommandSource, T>.Argument(name, type);
    }

    public static Task ExecuteCommandAsync(ServerPlayer player, string input) =>
        ExecuteCommandAsync(CommandSource.OfPlayer(player), Constants.CommandPrefix, input);

    public static Task ExecuteCommandByConsoleAsync(string input) =>
        ExecuteCommandAsync(CommandSource.OfConsole(), "", input);

    public static async Task ExecuteCommandAsync(CommandSource source, string prefix, string input)
    {
        var dispatcher = GameServer.Instance.Dispatcher;
        var cursorOffset = prefix.Length;
        var result = dispatcher.Parse(input[cursorOffset..], source);
        await ExecuteParseResultAsync(result, prefix, input, cursorOffset);
    }
    
    private static async Task ExecuteParseResultAsync(ParseResults<CommandSource> result, string prefix, 
        string input, int cursorOffset)
    {
        var dispatcher = GameServer.Instance.Dispatcher;
        var source = result.Context.GetSource();
        var sender = source.Sender;

        var exceptions = result.Exceptions.Values;
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

        var reader = result.Reader;
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
                var context = result.Context.GetLastChild();
                var usage = GetUsageText(dispatcher!, context, input, cursorOffset);
                if (!string.IsNullOrEmpty(usage))
                {
                    await sender!.SendErrorMessageAsync($"用法: {usage}");
                }
            }

            await Run();
            return;
        }

        var context = result.Context.GetLastChild();
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

        try
        {
            await dispatcher.ExecuteAsync(result);
        }
        catch (Exception ex)
        {
            await sender.SendErrorMessageAsync("執行指令時發生錯誤。");
            Logger.Error(ex.ToString());
        }
    }
    
    private static string GetUsageText(CommandDispatcher<CommandSource> dispatcher,
        CommandContextBuilder<CommandSource> context, string input, int cursorOffset)
    {
        var nodes = context.GetNodes();
        if (!nodes.Any()) return "";
        
        var node = nodes[^1];
        var parent = nodes.Count > 1 ? nodes[^2] : null;
        cursorOffset += node.GetRange().GetStart();
        var usage = dispatcher.GetSmartUsage(parent?.GetNode() ?? dispatcher.Root, context.GetSource());
        return input[..cursorOffset] + usage[node.GetNode()];
    }

    protected static async Task<bool> CheckSourceInSessionAsync(CommandContext<CommandSource> context)
    {
        var source = context.Source;
        var session = source.Session;
        if (session == null)
        {
            await source.SendErrorMessageAsync("你不在房間內。");
            return false;
        }

        return true;
    }
}