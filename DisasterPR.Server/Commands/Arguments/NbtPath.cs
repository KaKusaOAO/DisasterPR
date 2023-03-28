using System.Text;
using KaLib.Nbt;
using Microsoft.Extensions.Primitives;

namespace DisasterPR.Server.Commands.Arguments;

public class NbtPath
{
    private List<PathOperation> _operations;

    public NbtPath(List<PathOperation> operations)
    {
        _operations = operations;
    }
    
    public NbtTag Navigate(NbtTag tag) => 
        _operations.Aggregate(tag, (current, op) => op.Navigate(current));


    public string ToPathString()
    {
        var sb = new StringBuilder("$");
        foreach (var op in _operations)
        {
            if (op is ChildOperation)
            {
                sb.Append('.');
            }
            
            sb.Append(op.ToPathString());
        }

        return sb.ToString();
    }
}

public abstract class PathOperation
{
    public abstract NbtTag Navigate(NbtTag tag);
    public abstract string ToPathString();
}

public class IndexOperation : PathOperation
{
    public int Index { get; }

    public IndexOperation(int index)
    {
        Index = index;
    }
    
    public override NbtTag Navigate(NbtTag tag)
    {
        return tag switch
        {
            NbtList list => list[Index],
            NbtIntArray iArr => new NbtInt(iArr.Value[Index]),
            NbtByteArray bArr => new NbtByte(bArr.Value[Index]),
            NbtLongArray lArr => new NbtLong(lArr.Value[Index]),
            _ => throw new InvalidOperationException("Not a list or an array")
        };
    }

    public override string ToPathString() => $"[{Index}]";
}

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

        return name;
    }
}