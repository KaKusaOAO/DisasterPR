using Mochi.Utils;

namespace DisasterPR.Cards.Providers;

public class UpstreamPackProvider : UpstreamFormatPackProvider
{
    public const string DataPath = "https://disasterpr.fun/Game/DisasterPR_Beta1.286/cardpack.json";
    private bool _upstreamFailure;
    
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

    protected override async Task<Stream> GetStreamAsync()
    {
        if (_upstreamFailure)
        {
            Logger.Warn("Using fallback due to upstream failure.");
            return await GetStreamFromFallbackAsync();
        }
        
        Logger.Info("Getting upstream cardpack...");
        
        var stream = null as Stream;
        await Task.WhenAny(Task.Run(async () =>
        {
            var buffer = new MemoryStream();
            var s = await base.GetStreamAsync();
            await s.CopyToAsync(buffer);
            buffer.Position = 0;
            stream = buffer;
        }), Task.Delay(1000));

        if (stream != null)
        {
            Logger.Info("Fetching from upstream succeed.");
            return stream;
        }
        
        Logger.Warn("Fetching from upstream timed out! Using cached fallback...");
        return await GetStreamFromFallbackAsync();
    }

    private async Task<Stream> GetStreamFromFallbackAsync()
    {
        var http = new HttpClient();;
        return await http.GetStreamAsync($"http://{Constants.ServerHost}/u_game/packs/upstream.json");
    }

    public override CardPackBuilder MakeBuilder()
    {
        InvalidateExpiredCache();
        if (_cache != null) return _cache;

        var pack = null as CardPackBuilder;
        try
        {
            pack = base.MakeBuilder();
        }
        catch (Exception ex)
        {
            Logger.Warn("Failed to use upstream data! Using fallback...");
            Logger.Warn(ex.ToString());
            _upstreamFailure = true;
            pack = base.MakeBuilder();
        }
        
        _cache = pack.WithExplicitGuid(Guid.Empty);
        _lastUpdate = DateTimeOffset.Now;
        return _cache;
    }
}