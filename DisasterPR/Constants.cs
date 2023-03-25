namespace DisasterPR;

public static class Constants
{
    public const string ServerHost = "play.kakaouo.com";
    public const short ServerPort = 5221; 
    public static Uri ServerUri => new($"ws://{ServerHost}:{ServerPort}/gateway");
    public const int ProtocolVersion = 2;
    public const int SessionMaxPlayers = 8;

    public const string CommandPrefix = "/";

    public const bool EnableTestRoom = true;
    public const int TestRoomId = 1314520;
    public const int TestRoomPlayersCount = 3;
}