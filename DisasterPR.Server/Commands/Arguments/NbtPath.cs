using System.Text;
using Microsoft.Extensions.Primitives;
using Mochi.Nbt;

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
            sb.Append(op.ToPathString());
        }

        return sb.ToString();
    }
}

public class ConditionalOperation : PathOperation
{
    public PathOperation Operation { get; }

    public ConditionalOperation(PathOperation operation)
    {
        Operation = operation;
    }
    
    public override NbtTag Navigate(NbtTag tag)
    {
        if (tag == null!) return null!;
        
        try
        {
            return Operation.Navigate(tag);
        }
        catch
        {
            return null!;
        }
    }

    public override string ToPathString()
    {
        return "?" + Operation.ToPathString();
    }
}