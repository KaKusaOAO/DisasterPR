namespace DisasterPR.Server.Platforms;

public class PlainPlatformData : IPlatformData
{
    public event Action? Updated;
    public PlainPlatformData(ServerPlayer player)
    {
        Player = player;
    }
    
    public ServerPlayer Player { get; }
    public string Identifier => $"{Player.Id}";
    public byte[]? AvatarData => null;
}