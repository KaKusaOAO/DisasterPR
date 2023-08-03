using System.Net;
using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ServerboundLoginPacket : IPacket<IServerLoginPacketHandler>
{
    public PlayerPlatform Type { get; set; }
    public ILoginContent Content { get; set; } = null!;

    private ServerboundLoginPacket()
    {
        
    }
    
    public ServerboundLoginPacket(string name)
    {
        Type = PlayerPlatform.Plain;
        Content = new PlainLoginContent
        {
            PlayerName = name
        };
    }

    public static ServerboundLoginPacket CreateDiscord(string token)
    {
        return new ServerboundLoginPacket
        {
            Type = PlayerPlatform.Discord,
            Content = new DiscordLoginContent
            {
                AccessToken = token
            }
        };
    }

    public T? GetContent<T>() where T : class, ILoginContent => Content as T; 
    
    public ServerboundLoginPacket(PacketContent content)
    {
        void CreateContent()
        {
            Content = Type switch
            {
                PlayerPlatform.Plain => new PlainLoginContent(),
                PlayerPlatform.Discord => new DiscordLoginContent(),
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            };
        }
        
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Type = stream.ReadEnum<PlayerPlatform>();
            CreateContent();
            Content.ReadFromStream(stream);
        }
        else
        {
            var payload = content.GetAsJsonObject();
            Type = (PlayerPlatform) payload["type"]!.GetValue<int>();
            CreateContent();
            Content.ReadFromJson(payload);
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteEnum(Type);
        Content.WriteToStream(stream);
    }

    public void Write(JsonObject obj)
    {
        obj["type"] = (int) Type;
        Content.WriteToJson(obj);
    }

    public void Handle(IServerLoginPacketHandler handler) => handler.HandleLogin(this);
}

public interface ILoginContent
{
    public PlayerPlatform Type { get; }
    public void ReadFromStream(BufferReader stream);
    public void ReadFromJson(JsonObject obj);
    public void WriteToStream(BufferWriter stream);
    public void WriteToJson(JsonObject obj);
}

public class PlainLoginContent : ILoginContent
{
    public PlayerPlatform Type => PlayerPlatform.Plain;
    public string PlayerName { get; set; }
    
    public void ReadFromStream(BufferReader stream)
    {
        PlayerName = stream.ReadUtf8String();
    }

    public void ReadFromJson(JsonObject obj)
    {
        PlayerName = obj["name"]!.GetValue<string>();
    }

    public void WriteToStream(BufferWriter stream)
    {
        stream.WriteUtf8String(PlayerName);
    }

    public void WriteToJson(JsonObject obj)
    {
        obj["name"] = PlayerName;
    }
}

public class DiscordLoginContent : ILoginContent
{
    public PlayerPlatform Type => PlayerPlatform.Discord;
    public string AccessToken { get; set; }
    
    public void ReadFromStream(BufferReader stream)
    {
        AccessToken = stream.ReadUtf8String();
    }

    public void ReadFromJson(JsonObject obj)
    {
        AccessToken = obj["accessToken"]!.GetValue<string>();
    }

    public void WriteToStream(BufferWriter stream)
    {
        stream.WriteUtf8String(AccessToken);
    }

    public void WriteToJson(JsonObject obj)
    {
        obj["accessToken"] = AccessToken;
    }
}