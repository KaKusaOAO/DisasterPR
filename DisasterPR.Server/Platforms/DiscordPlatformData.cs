using DisasterPR.Server.Platforms.Discord;
using Mochi.Utils;

namespace DisasterPR.Server.Platforms;

public class DiscordPlatformData : IPlatformData
{
    public event Action? Updated;
    public DiscordPlatformData(DiscordUserModel user)
    {
        User = user;
        _ = DownloadAvatarAsync();
    }

    private async Task DownloadAvatarAsync()
    {
        var client = new HttpClient();
        await using var stream = await client.GetStreamAsync(User.GetAvatarUrl());

        var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        
        Logger.Info($"Downloaded avatar from {User.GetAvatarUrl()}");
        AvatarData = buffer.GetBuffer();
        Updated?.Invoke();
    }
    
    public DiscordUserModel User { get; }

    public string Identifier => User.HasLegacyUsername()
        ? $"#dc:#{User.Id}"
        : $"#dc:@{User.Username}";

    public byte[]? AvatarData { get; set; }
}