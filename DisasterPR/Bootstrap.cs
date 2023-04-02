using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using Mochi.Utils;

namespace DisasterPR;

public static class Bootstrap
{
    private static bool _bootstrapped;
    
    public static void Boot()
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        
        Logger.Info("Prefetching upstream cardpack...");
        IPackProvider.Upstream.Make();
        Logger.Info("Upstream pack fetched.");
    }
}