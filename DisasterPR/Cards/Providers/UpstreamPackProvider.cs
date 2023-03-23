namespace DisasterPR.Cards.Providers;

public class UpstreamPackProvider : UpstreamFormatPackProvider
{
    public const string DataPath = "https://disasterpr.fun/Game/DisasterPR_Beta1.286/cardpack.json";
    
    public UpstreamPackProvider() : base(new Uri(DataPath))
    {
    }

    private static CardPackBuilder? _cache;
    private static DateTimeOffset _lastUpdate = DateTimeOffset.UnixEpoch;

    private static void InvalidateExpiredCache()
    {
        if (_cache == null) return;
        
        if (DateTimeOffset.Now - _lastUpdate > TimeSpan.FromDays(1))
        {
            _cache = null;
        }
    }
    
    public override async Task<CardPackBuilder> MakeBuilderAsync()
    {
        InvalidateExpiredCache();
        if (_cache != null) return _cache;

        var pack = await base.MakeBuilderAsync();
        _cache = pack.WithExplicitGuid(Guid.Empty);
        _lastUpdate = DateTimeOffset.Now;
        return _cache;
    }
}