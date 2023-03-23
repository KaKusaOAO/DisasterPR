using KaLib.Utils;

namespace DisasterPR.Server.Commands.Senders;

public class ConsoleCommandSender : ICommandSender
{
    public string Name => "Console";
    
    public Task SendMessageAsync(string content)
    {
        Logger.Info(content);
        return Task.CompletedTask;
    }

    public Task SendErrorMessageAsync(string content)
    {
        Logger.Error(content);
        return Task.CompletedTask;
    }
}