namespace DisasterPR.Cards.Providers;

public static class PackProviderExtension
{
    public static async Task<CardPackBuilder> MakeBuilderAsync(this IPackProvider provider)
    {
        await Task.Yield();
        return provider.MakeBuilder();
    }
    
    public static async Task<CardPack> MakeAsync(this IPackProvider provider)
    {
        var builder = await provider.MakeBuilderAsync();
        return builder.Build();
    }

    public static CardPack Make(this IPackProvider provider) => provider.MakeBuilder().Build();
}