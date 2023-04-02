using Mochi.Nbt;

namespace DisasterPR.Server.Commands.Arguments;

public abstract class PathOperation
{
    public abstract NbtTag Navigate(NbtTag tag);
    public abstract string ToPathString();
}