namespace DisasterPR.Cards.Providers;

public class UpstreamPackProvider : UpstreamFormatPackProvider
{
    public const string DataPath = "https://disasterpr.fun/Game/DisasterPR_Beta1.286/cardpack.json";
    
    public UpstreamPackProvider() : base(new Uri(DataPath))
    {
    }

    private static CardPack? _cache;
    private static DateTimeOffset _lastUpdate = DateTimeOffset.UnixEpoch;

    private static void InvalidateExpiredCache()
    {
        if (_cache == null) return;
        
        if (DateTimeOffset.Now - _lastUpdate > TimeSpan.FromDays(1))
        {
            _cache = null;
        }
    }
    
    public override async Task<CardPack> MakeAsync()
    {
        InvalidateExpiredCache();
        if (_cache != null) return _cache;

        var pack = await base.MakeAsync();
        _cache = new CardPack(Guid.NewGuid(), pack.Categories, pack.Topics, pack.Words);
        _lastUpdate = DateTimeOffset.Now;
        return _cache;
    }
}