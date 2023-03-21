namespace DisasterPR.Client;

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