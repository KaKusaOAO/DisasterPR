namespace DisasterPR;

public static class Constants
{
    public static Uri ServerUri => new Uri("ws://localhost:5221/gateway");
    public const int ProtocolVersion = 1;
    public const int SessionMaxPlayers = 8;
}