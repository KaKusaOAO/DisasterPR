using KaLib.Nbt;

namespace DisasterPR.Server.Commands.Arguments;

public class ChildOperation : PathOperation
{
    public string PropertyName { get; }

    public ChildOperation(string propertyName)
    {
        PropertyName = propertyName;
    }
    
    public override NbtTag Navigate(NbtTag tag)
    {
        if (tag is not NbtCompound c) throw new InvalidOperationException("Not a compound");
        return c[PropertyName];
    }
    
    public override string ToPathString()
    {
        var name = PropertyName;

        if (string.IsNullOrEmpty(name))
        {
            return "\"\"";
        }
        
        if (name.Contains(' '))
        {
            name = name.Replace("\\", "\\\\").Replace("\"", "\\\"");
            name = $"\"{name}\"";
        }

        return "." + name;
    }
}