namespace DisasterPR;

public static class Constants
{
    public static Uri ServerUri => new Uri("ws://play.kakaouo.com:5221/gateway");
    public const int ProtocolVersion = 1;
    public const int SessionMaxPlayers = 8;

    public const bool EnableTestRoom = true;
    public const int TestRoomId = 1453;
    public const int TestRoomPlayersCount = 3;
}