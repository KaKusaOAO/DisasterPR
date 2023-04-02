using DisasterPR.Server.Commands.Senders;
using Mochi.Brigadier;

namespace DisasterPR.Server.Commands;

public interface IRegisteredCommand
{
    public static abstract void Register(CommandDispatcher<CommandSource> d);
}