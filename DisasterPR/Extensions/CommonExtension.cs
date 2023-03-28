namespace DisasterPR.Extensions;

public static class CommonExtension
{
    public static List<T> Shuffled<T>(this IEnumerable<T> arr, int? seed = null)
    {
        var shuffled = arr.ToArray();
        shuffled.Shuffle(seed);
        return shuffled.ToList();
    }
    
    public static void Shuffle<T>(this T[] arr, int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        
        for (var i = 0; i < arr.Length; i++)
        {
            var j = random.Next(arr.Length);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
    
    public static int IndexOf<T>(this T[] arr, T obj)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            if (Equals(obj, arr[i])) return i;
        }

        return -1;
    }

    public static TValue? AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            var old = dict[key];
            dict[key] = value;
            return old;
        }
        
        dict.Add(key, value);
        return default;
    }
}