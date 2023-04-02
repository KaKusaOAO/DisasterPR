using Mochi.Nbt;

namespace DisasterPR.Server.Commands.Arguments;

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

public class SelectOperation : PathOperation
{
    public NbtPath Path { get; }

    public SelectOperation(NbtPath path)
    {
        Path = path;
    }
    
    public override NbtTag Navigate(NbtTag tag)
    {
        if (tag is not NbtList list)
            throw new InvalidOperationException("Not a list");

        var result = new NbtList();
        foreach (var item in list)
        {
            result.Add(Path.Navigate(item));
        }
        return result;
    }

    public override string ToPathString() => $"[{Path.ToPathString()}]";
}