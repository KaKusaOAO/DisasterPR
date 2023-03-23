using DisasterPR.Server.Commands.Senders;
using KaLib.Brigadier;

namespace DisasterPR.Server.Commands;

public interface IRegisteredCommand
{
    public static abstract void Register(CommandDispatcher<IServerCommandSource> d);
}