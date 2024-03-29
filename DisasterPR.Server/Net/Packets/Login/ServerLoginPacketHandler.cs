using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Server.Controllers;
using DisasterPR.Server.Platforms;
using DisasterPR.Server.Platforms.Discord;
using Mochi.Texts;
using Mochi.Utils;
using LogLevel = Mochi.Utils.LogLevel;

namespace DisasterPR.Server.Net.Packets.Login;

public class ServerLoginPacketHandler : IServerLoginPacketHandler
{
    private class AccessTokenExchangePayload
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }
        
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }
        
        [JsonPropertyName("code")]
        public string Code { get; set; }
        
        [JsonPropertyName("redirect_uri")]
        public string RedirectUri { get; set; }
    }

    private class AccessTokenResponsePayload
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
    
    public ServerToPlayerConnection Connection { get; }
    public ServerPlayer Player => Connection.Player;

    public ServerLoginPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    public async void HandleLogin(ServerboundLoginPacket packet)
    {
        var version = Connection.ProtocolVersion;
        var shouldDisconnect = false;

        async Task DisconnectAsync(PlayerKickReason reason)
        {
            shouldDisconnect = true;
            await Connection.SendPacketAsync(new ClientboundDisconnectPacket(reason));
            await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        try
        {
            if (version > Constants.ProtocolVersion)
            {
                Logger.Warn("The server is too old!");
                await DisconnectAsync(PlayerKickReason.ServerTooOld);
                return;
            }

            if (version < Constants.ProtocolVersion)
            {
                Logger.Warn("The client is too old!");
                await DisconnectAsync(PlayerKickReason.ClientTooOld);
                return;
            }

            if (packet.Type == PlayerPlatform.Plain)
            {
                if (!PlayerName.IsValid(packet.GetContent<PlainLoginContent>()!.PlayerName))
                {
                    Logger.Warn("The player name is invalid!");
                    await DisconnectAsync(PlayerKickReason.InvalidName);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // Discard exceptions if we have already disconnected
            if (shouldDisconnect) return;

            // Otherwise, log the exception because something is definitely broken
            Logger.Warn("Failed to validate player!");
            Logger.Warn(ex);
        }

        string name;
        if (packet.Type == PlayerPlatform.Plain)
        {
            name = PlayerName.ProcessName(packet.GetContent<PlainLoginContent>()!.PlayerName);
            Player.Name = name;
            Player.PlatformData = new PlainPlatformData(Player);
        } else if (packet.Type == PlayerPlatform.Discord)
        {
            var codeContext = Uri.UnescapeDataString(packet.GetContent<DiscordLoginContent>()!.AccessToken);
            var noPopup = codeContext.StartsWith("1:");
            var isStandalone = codeContext.StartsWith("2:");
            if (!noPopup && !isStandalone && !codeContext.StartsWith("0:"))
            {
                await Player.SendToastAsync("無效的 Discord 登入資訊，請重新嘗試登入。", LogLevel.Error);
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket("驗證失敗！"));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }
            
            var code = codeContext[2..];
            var client = new HttpClient();
            
            Logger.Log("Exchanging Discord access token with OAuth code...");
            var content = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(
                new AccessTokenExchangePayload
                {
                    ClientId = DiscordApiConstants.ClientId.ToString(),
                    ClientSecret = DiscordApiConstants.ClientSecret,
                    GrantType = "authorization_code",
                    Code = code,
                    RedirectUri = isStandalone ? "http://localhost:61357/" : DiscordApiConstants.RedirectUri + (noPopup ? "?nopopup" : "")
                }))!;
            var codePayload = new FormUrlEncodedContent(content);
        
            var result = await client.PostAsync("https://discord.com/api/oauth2/token", codePayload);
            if (!result.IsSuccessStatusCode)
            {
                Logger.Warn("Invalid Discord OAuth code! Maybe it is expired or malformed?");
                Logger.Warn(await result.Content.ReadAsStringAsync());

                await Player.SendToastAsync("Discord 登入資訊驗證失敗！請重新嘗試登入。", LogLevel.Error);
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket("驗證失敗！"));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }
            
            Logger.Log("Success!");
            var response = (await result.Content.ReadFromJsonAsync<AccessTokenResponsePayload>())!;
            var token = response.AccessToken;
            
            // Add the fetched token to the header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = await client.GetAsync("https://discord.com/api/users/@me");
            if (!payload.IsSuccessStatusCode)
            {
                Logger.Warn("Discord authentication failed!");

                var str = await payload.Content.ReadAsStringAsync();
                Logger.Warn(str);
                await Player.SendToastAsync("Discord 登入資訊驗證失敗！請清除 Cookie 後重新嘗試登入。", LogLevel.Error);
                await Connection.SendPacketAsync(new ClientboundDisconnectPacket("驗證失敗！"));
                await Connection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return;
            }

            var user = (await payload.Content.ReadFromJsonAsync<DiscordUserModel>())!;
            Player.PlatformData = new DiscordPlatformData(user);
            var tag = user.Username;
            if (user.Discriminator is null or "0")
            {
                tag = user.GlobalName + " (" + tag + ")";
                name = user.GlobalName ?? user.Username;
            } else
            {
                tag += "#" + user.Discriminator;
                name = user.Username;
            }
            Logger.Info(TranslateText.Of("Discord user logged in as %s")
                .AddWith(LiteralText.Of(tag).SetColor(TextColor.Gold)));

            var a = BitConverter.GetBytes(user.Id);
            var arr = new byte[16];
            Array.Copy(a, 0, arr, 8, 8);
            Player.Id = new Guid(arr);

            name = PlayerName.ProcessDiscordName(name);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(packet.Type), packet.Type, null);
        }

        Player.LoginType = packet.Type;
        Player.Name = name;
        Player.DefaultName = name;
        Logger.Verbose(TranslateText.Of("Player %s ID is %s")
            .AddWith(LiteralText.Of(name).SetColor(TextColor.Gold))
            .AddWith(LiteralText.Of(Player.Id.ToString()).SetColor(TextColor.Green))
        );
        await Connection.SendPacketAsync(new ClientboundAckLoginPacket(Player));
        Connection.CurrentState = PacketState.Play;
        
        await Player.SendToastAsync($"歡迎，{name}！");
    }
}