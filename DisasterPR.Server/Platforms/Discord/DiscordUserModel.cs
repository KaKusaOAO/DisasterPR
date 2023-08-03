using System.Text.Json.Serialization;

namespace DisasterPR.Server.Platforms.Discord;

public class DiscordUserModel
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong Id { get; set; }
        
    [JsonPropertyName("username")]
    public string Username { get; set; }
        
    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }
        
    [JsonPropertyName("discriminator")]
    public string? Discriminator { get; set; }
    
    [JsonPropertyName("avatar")]
    public string? AvatarHash { get; set; }

    public bool HasLegacyUsername() => Discriminator != null && Discriminator != "0";

    public string GetAvatarUrl()
    {
        const string cdnUrl = "https://cdn.discordapp.com/";
        if (AvatarHash != null) return $"{cdnUrl}avatars/{Id}/{AvatarHash}.png?size=256";
        return HasLegacyUsername()
            ? $"{cdnUrl}embed/avatars/{ushort.Parse(Discriminator!) % 5}.png"
            : $"{cdnUrl}embed/avatars/{(Id >> 22) % 6}.png";
    }
}