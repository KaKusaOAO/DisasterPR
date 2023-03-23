namespace DisasterPR.Cards.Providers;

public static class PackProviderExtension
{
    public static async Task<CardPack> MakeAsync(this IPackProvider provider)
    {
        var builder = await provider.MakeBuilderAsync();
        return builder.Build();
    }
}