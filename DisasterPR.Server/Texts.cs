using DisasterPR.Server.Commands.Senders;
using KaLib.Texts;
using ISession = DisasterPR.Sessions.ISession;

namespace DisasterPR.Server;

public static class Texts
{
    public static IText OfSender(ICommandSender? sender) => LiteralText.Of(sender?.Name ?? "<null>").SetColor(TextColor.Gold);

    public static IText OfSession(ISession? session)
    {
        return LiteralText.Of($"Session #{session?.RoomId.ToString() ?? "<null>"}").SetColor(TextColor.Green);
    }
}