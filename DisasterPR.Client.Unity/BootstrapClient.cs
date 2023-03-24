namespace DisasterPR.Client.Unity;

public static class BootstrapClient
{
    static BootstrapClient()
    {
        _ = Game.Instance;
    }

    public static void EnsureInit()
    {
        
    }
}