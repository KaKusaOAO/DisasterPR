using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Login;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Server.Net.Packets.Login;

public class ServerLoginPacketHandler : IServerLoginPacketHandler
{
    public ServerToPlayerConnection Connection { get; }
    public ServerPlayer Player => Connection.Player;

    public ServerLoginPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }

    private class DiscordUserModel
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

            if (packet.Type == ServerboundLoginPacket.LoginType.Plain)
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
        if (packet.Type == ServerboundLoginPacket.LoginType.Plain)
        {
            name = PlayerName.ProcessName(packet.GetContent<PlainLoginContent>()!.PlayerName);
            Player.Name = name;
        } else if (packet.Type == ServerboundLoginPacket.LoginType.Discord)
        {
            var token = packet.GetContent<DiscordLoginContent>()!.AccessToken;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var user = (await client.GetFromJsonAsync<DiscordUserModel>("https://discord.com/api/users/@me"))!;

            if (user.Discriminator is null or "0")
            {
                name = user.GlobalName ?? user.Username;
            }
            else
            {
                name = user.Username;
            }

            var a = BitConverter.GetBytes(user.Id);
            var arr = new byte[16];
            Array.Copy(a, 0, arr, 8, 8);
            Player.Id = new Guid(arr);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(packet.Type), packet.Type, null);
        }

        Logger.Verbose(TranslateText.Of("Player %s ID is %s")
            .AddWith(LiteralText.Of(name).SetColor(TextColor.Gold))
            .AddWith(LiteralText.Of(Player.Id.ToString()).SetColor(TextColor.Green))
        );
        await Connection.SendPacketAsync(new ClientboundAckLoginPacket(Player.Id, name));
        Connection.CurrentState = PacketState.Play;
        
        await Player.SendToastAsync($"歡迎，{name}！");
    }
}