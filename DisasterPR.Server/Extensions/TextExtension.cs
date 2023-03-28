using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using KaLib.Texts;

namespace DisasterPR.Server.Extensions;

public static class TextExtension
{
    public static JsonObject ToJson(this IText text)
    {
        var obj = new JsonObject();
        var extras = new JsonArray();
        foreach (var e in text.Extra)
        {
            extras.Add(e.ToJson());
        }

        obj.AddOrSet("extra", extras);

        if (text.Color != null)
        {
            obj.AddOrSet("color", "#" + text.Color.Color.RGB.ToString("x6"));
        }

        if (text is LiteralText literal)
        {
            obj.AddOrSet("text", literal.Text);
        }

        if (text is TranslateText translate)
        {
            var withs = new JsonArray();
            foreach (var w in translate.With)
            {
                withs.Add(w.ToJson());
            }
            
            obj.AddOrSet("translate", translate.Translate);
            obj.AddOrSet("with", withs);
        }

        return obj;
    }
}