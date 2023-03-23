namespace DisasterPR.Cards.Providers;

public class LazyLambdaPackProvider : IPackProvider
{
    private Lazy<Task<CardPackBuilder>> _lazy;

    public LazyLambdaPackProvider(Func<Task<CardPackBuilder>> func)
    {
        _lazy = new Lazy<Task<CardPackBuilder>>(func);
    }
    
    public Task<CardPackBuilder> MakeBuilderAsync()
    {
        return _lazy.Value;
    }
}