using DisasterPR.Proxy.Commands.Senders;
using KaLib.Brigadier;

namespace DisasterPR.Proxy.Commands;

public interface IRegisteredCommand
{
    public static abstract void Register(CommandDispatcher<CommandSource> d);
}