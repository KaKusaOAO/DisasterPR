using DisasterPR.Extensions;

namespace DisasterPR;

public class ShuffledPool<T>
{
    private T[] _arr;
    public int Index { get; set; }

    public ShuffledPool(IEnumerable<T> things)
    {
        _arr = things.Shuffled().ToArray();
    }

    public void Reset()
    {
        _arr.Shuffle();
        Index = 0;
    }

    public T Next()
    {
        var item = _arr[Index];
        Index++;
        Index %= _arr.Length;
        return item;
    }

    public IEnumerable<T> Next(int count) => Enumerable.Range(0, count).Select(_ => Next());

    public List<T> Items => _arr.ToList();
}