using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DisasterPR.Server.Controllers;

public class DiscordAuthorizeController : ControllerBase
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
    
    [Route("/discord/authorize")]
    public async Task Get(string code)
    {
        // Make a request to exchange for the access token
        var client = new HttpClient();

        var content = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(
            new AccessTokenExchangePayload
            {
                ClientId = DiscordApiConstants.ClientId.ToString(),
                ClientSecret = DiscordApiConstants.ClientSecret,
                GrantType = "authorization_code",
                Code = code,
                RedirectUri = DiscordApiConstants.RedirectUri
            }))!;
        var payload = new FormUrlEncodedContent(content);
        var result = await client.PostAsync("https://discord.com/api/oauth2/token", payload);
        var response = (await result.Content.ReadFromJsonAsync<AccessTokenResponsePayload>())!;

        Response.Cookies.Append("access_token", response.AccessToken, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(response.ExpiresIn)
        });

        var body = Response.Body;
        var writer = new StreamWriter(body);
        await writer.WriteLineAsync("<script>");
        await writer.WriteLineAsync("history.replaceState({}, null, '?');");
        await writer.WriteLineAsync("location.href = 'http://play.kakaouo.com/disasterpr/discord/authorized';");
        await writer.WriteLineAsync("</script>");
        await writer.FlushAsync();
    }
}