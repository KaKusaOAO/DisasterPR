namespace DisasterPR.Cards.Providers;

public class LambdaPackProvider : IPackProvider
{
    private Func<CardPackBuilder> _func;

    public LambdaPackProvider(Func<Task<CardPackBuilder>> func)
    {
        _func = () => func().Result;
    }
    
    public LambdaPackProvider(Func<CardPackBuilder> func)
    {
        _func = func;
    }
    
    public CardPackBuilder MakeBuilder()
    {
        return _func();
    }
}