using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Mochi.Utils;

namespace DisasterPR.Server.Controllers;

public class DiscordAuthorizeController : ControllerBase
{
    [Route("/discord/authorize")]
    public async Task Get(string code)
    {
        // Store the code to the cookie.
        // We will use it to exchange the token later.
        Response.Cookies.Append("access_token", code, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(5) // response.ExpiresIn)
        });

        var noPopup = Request.Query.ContainsKey("nopopup");

        var body = Response.Body;
        var writer = new StreamWriter(body);
        await writer.WriteLineAsync("<script>");
        await writer.WriteLineAsync("history.replaceState({}, null, '?');");
        
        if (noPopup)
        {
            await writer.WriteLineAsync("location.href = 'http://play.kakaouo.com/u_game/';");
        }
        else
        {
            await writer.WriteLineAsync("location.href = 'http://play.kakaouo.com/u_game/discord/authorized';");
        }

        await writer.WriteLineAsync("</script>");
        await writer.FlushAsync();
    }
}