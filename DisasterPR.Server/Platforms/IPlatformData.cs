namespace DisasterPR.Server.Platforms;

public interface IPlatformData
{
    public event Action Updated;
    public string Identifier { get; }
    public byte[]? AvatarData { get; }
}