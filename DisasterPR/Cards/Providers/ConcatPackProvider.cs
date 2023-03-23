namespace DisasterPR.Cards.Providers;

public class ConcatPackProvider : IPackProvider
{
    public List<IPackProvider> Providers { get; } = new();
    
    public async Task<CardPackBuilder> MakeBuilderAsync()
    {
        var builders = new List<CardPackBuilder>();
        foreach (var provider in Providers)
        {
            var b = await provider.MakeBuilderAsync();
            builders.Add(b);
        }

        return builders.Aggregate(CardPackBuilder.Create(), (a, b) =>
        {
            a.Categories.AddRange(b.Categories);
            a.Topics.AddRange(b.Topics);
            a.Words.AddRange(b.Words);
            return a;
        });
    }
}