using DisasterPR.Server.Sessions;

namespace DisasterPR.Server.Commands.Senders;

public class ServerPlayerCommandSource : IServerCommandSource
{
    public ServerPlayer Player { get; init; }

    public ICommandSender Sender => Player;
    public ServerSession? Session => Player.Session;

    public Task SendMessageAsync(string content) => Player.SendMessageAsync(content);

    public Task SendErrorMessageAsync(string content) => Player.SendErrorMessageAsync(content);
}