namespace DisasterPR.Cards.Providers;

public class LazyLambdaPackProvider : IPackProvider
{
    private Lazy<CardPackBuilder> _lazy;

    public LazyLambdaPackProvider(Func<Task<CardPackBuilder>> func)
    {
        _lazy = new Lazy<CardPackBuilder>(() => func().Result);
    }
    
    public LazyLambdaPackProvider(Func<CardPackBuilder> func)
    {
        _lazy = new Lazy<CardPackBuilder>(func);
    }
    
    public CardPackBuilder MakeBuilder()
    {
        return _lazy.Value;
    }
}