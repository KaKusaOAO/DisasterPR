namespace DisasterPR;

public static class Utils
{
    public static T Modify<T>(T initial, Action<T> modifier)
    {
        modifier(initial);
        return initial;
    }
    
    public static T Init<T>(Action<T> initializer) where T : new()
    {
        var initial = new T();
        initializer(initial);
        return initial;
    }
}