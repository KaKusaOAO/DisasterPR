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

        obj["extra"] = extras;

        if (text.Color != null)
        {
            obj["color"] = "#" + text.Color.Color.RGB.ToString("x6");
        }

        switch (text)
        {
            case LiteralText literal:
                obj["text"] = literal.Text;
                break;
            case TranslateText translate:
            {
                var withs = new JsonArray();
                foreach (var w in translate.With)
                {
                    withs.Add(w.ToJson());
                }

                obj["translate"] = translate.Translate;
                obj["with"] = withs;
                break;
            }
        }

        return obj;
    }
}