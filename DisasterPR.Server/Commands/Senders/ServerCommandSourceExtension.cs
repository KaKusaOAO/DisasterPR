using KaLib.Texts;
using KaLib.Utils;

namespace DisasterPR.Server.Commands.Senders;

public static class ServerCommandSourceExtension
{
    public static Task SendMessageAsync(this IServerCommandSource source, string content) 
    {
        Logger.Verbose(TranslateText.Of("[%s@%s: %s]")
            .AddWith(LiteralText.Of(source.Sender.Name))
            .AddWith(LiteralText.Of(source.Session?.RoomId.ToString() ?? "<null>"))
            .AddWith(LiteralText.Of(content)));
        return source.Sender.SendMessageAsync(content);
    }

    public static Task SendErrorMessageAsync(this IServerCommandSource source, string content) =>
        source.Sender.SendErrorMessageAsync(content);
}