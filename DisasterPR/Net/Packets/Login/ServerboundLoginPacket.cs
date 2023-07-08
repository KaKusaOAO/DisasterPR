using System.Net;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ServerboundLoginPacket : IPacket<IServerLoginPacketHandler>
{
    public LoginType Type { get; set; }
    public ILoginContent Content { get; set; } = null!;

    public enum LoginType
    {
        Plain,
        Discord
    }

    private ServerboundLoginPacket()
    {
        
    }
    
    public ServerboundLoginPacket(string name)
    {
        Type = LoginType.Plain;
        Content = new PlainLoginContent
        {
            PlayerName = name
        };
    }

    public static ServerboundLoginPacket CreateDiscord(string token)
    {
        return new ServerboundLoginPacket
        {
            Type = LoginType.Discord,
            Content = new DiscordLoginContent
            {
                AccessToken = token
            }
        };
    }

    public T? GetContent<T>() where T : class, ILoginContent => Content as T; 
    
    public ServerboundLoginPacket(BufferReader stream)
    {
        Type = stream.ReadEnum<LoginType>();

        Content = Type switch
        {
            LoginType.Plain => new PlainLoginContent(),
            LoginType.Discord => new DiscordLoginContent(),
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
        };

        Content.ReadFromStream(stream);
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteEnum(Type);
        Content.WriteToStream(stream);
    }

    public void Handle(IServerLoginPacketHandler handler) => handler.HandleLogin(this);
}

public interface ILoginContent
{
    public ServerboundLoginPacket.LoginType Type { get; }
    public void ReadFromStream(BufferReader stream);
    public void WriteToStream(BufferWriter stream);
}

public class PlainLoginContent : ILoginContent
{
    public ServerboundLoginPacket.LoginType Type => ServerboundLoginPacket.LoginType.Plain;
    public string PlayerName { get; set; }
    
    public void ReadFromStream(BufferReader stream)
    {
        PlayerName = stream.ReadUtf8String();
    }

    public void WriteToStream(BufferWriter stream)
    {
        stream.WriteUtf8String(PlayerName);
    }
}

public class DiscordLoginContent : ILoginContent
{
    public ServerboundLoginPacket.LoginType Type => ServerboundLoginPacket.LoginType.Discord;
    public string AccessToken { get; set; }
    
    public void ReadFromStream(BufferReader stream)
    {
        AccessToken = stream.ReadUtf8String();
    }

    public void WriteToStream(BufferWriter stream)
    {
        stream.WriteUtf8String(AccessToken);
    }
}