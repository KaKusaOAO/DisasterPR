namespace DisasterPR.Proxy.Commands.Senders;

public interface ICommandSender
{
    public string Name { get; }
    
    public Task SendMessageAsync(string content);
    public Task SendErrorMessageAsync(string content);
}