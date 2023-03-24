namespace DisasterPR.Cards.Providers;

public class ConcatPackProvider : IPackProvider
{
    public List<IPackProvider> Providers { get; } = new();

    public ConcatPackProvider(params IPackProvider[] providers)
    {
        Providers.AddRange(providers);
    }
    
    public CardPackBuilder MakeBuilder()
    {
        var builders = Providers.Select(provider => provider.MakeBuilder()).ToList();

        return builders.Aggregate(CardPackBuilder.Create(), (a, b) =>
        {
            a.Categories.AddRange(b.Categories);
            a.Topics.AddRange(b.Topics);
            a.Words.AddRange(b.Words);
            return a;
        });
    }
}