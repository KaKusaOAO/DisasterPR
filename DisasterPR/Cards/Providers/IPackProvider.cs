namespace DisasterPR.Cards.Providers;

public interface IPackProvider
{
    public static IPackProvider Upstream => new UpstreamPackProvider();

    public static IPackProvider Default => new UpstreamPackProvider();
    
    public Task<CardPack> MakeAsync();
}