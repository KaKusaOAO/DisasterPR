namespace DisasterPR.Cards.Providers;

public class LambdaPackProvider : IPackProvider
{
    private Func<Task<CardPackBuilder>> _func;

    public LambdaPackProvider(Func<Task<CardPackBuilder>> func)
    {
        _func = func;
    }
    
    public Task<CardPackBuilder> MakeBuilderAsync()
    {
        return _func();
    }
}