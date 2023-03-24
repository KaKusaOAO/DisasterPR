using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using KaLib.Utils;

namespace DisasterPR;

public static class Bootstrap
{
    private static bool _bootstrapped;
    
    public static async Task BootAsync()
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        
        Logger.Info("Prefetching upstream cardpack...");
        await IPackProvider.Upstream.MakeAsync();
    }
}