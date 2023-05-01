namespace DisasterPR.Extensions;

public static class CommonExtension
{
    public static List<T> Shuffled<T>(this IEnumerable<T> arr, int? seed = null)
    {
        var shuffled = arr.ToList();
        shuffled.Shuffle(seed);
        return shuffled;
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
    
    public static void Shuffle<T>(this IList<T> arr, int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        
        for (var i = 0; i < arr.Count; i++)
        {
            var j = random.Next(arr.Count);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    public static T Random<T>(this IList<T> arr, int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        return arr[random.Next(arr.Count)];
    }
    
    public static int IndexOf<T>(this T[] arr, T obj)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            if (Equals(obj, arr[i])) return i;
        }

        return -1;
    }

    /// <summary>
    /// Add or set the value to the dictionary. Returns the previous value if exists. <br/>
    /// If you are only updating the value, consider using the indexer instead.
    /// </summary>
    /// <param name="dict">The dictionary.</param>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The value to be set.</param>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>The previous set value if exists.</returns>
    public static TValue? AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        dict.TryGetValue(key, out var result);
        dict[key] = value;
        return result;
    }
}