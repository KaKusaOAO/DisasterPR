using System.Text;
using Mochi.Nbt;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Server.Extensions;

public static class NbtExtension
{
    private static string Repeat(this string str, int count)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            sb.Append(str);
        }

        return sb.ToString();
    }
    
    public static IText ToText(this NbtTag tag, bool formatted = false, int indent = 0)
    {
        if (tag == null!) return LiteralText.Of("<null>").SetColor(TextColor.Red);

        switch (tag.Type)
        {
            case NbtTag.TagType.List:
            {
                var text = LiteralText.Of("[");
                var addComma = false;
                foreach (var item in (NbtList) tag)
                {
                    if (addComma) text.AddExtra(LiteralText.Of(", "));
                    if (formatted) text.AddExtra(LiteralText.Of("\n" + "    ".Repeat(indent + 1)));
                    text.AddExtra(item.ToText(formatted, indent + 1));
                    addComma = true;
                }

                if (formatted && ((NbtList) tag).Any()) 
                    text.AddExtra(LiteralText.Of("\n" + "    ".Repeat(indent)));
                text.AddExtra(LiteralText.Of("]"));
                return text;
            }

            case NbtTag.TagType.Byte:
            {
                return Text.Represent(((NbtByte) tag).Value);
            }

            case NbtTag.TagType.Int:
            {
                return Text.Represent(((NbtInt) tag).Value);
            }

            case NbtTag.TagType.Compound:
            {
                var text = LiteralText.Of("{");
                var addComma = false;
                foreach (var (key, value) in (NbtCompound) tag)
                {
                    if (addComma) text.AddExtra(LiteralText.Of(", "));
                    if (formatted) text.AddExtra(LiteralText.Of("\n" + "    ".Repeat(indent + 1)));
                    text.AddExtra(TranslateText.Of("%s: %s")
                        .AddWith(LiteralText.Of(key).SetColor(TextColor.Aqua))
                        .AddWith(value.ToText(formatted, indent + 1))
                    );
                    addComma = true;
                }

                if (formatted && ((NbtCompound) tag).Any()) 
                    text.AddExtra(LiteralText.Of("\n" + "    ".Repeat(indent)));
                text.AddExtra(LiteralText.Of("}"));
                return text;
            }

            case NbtTag.TagType.String:
            {
                return Text.Represent(((NbtString) tag).Value);
            }

            default:
            {
                Logger.Warn($"Text converted not implemented for type {tag.Type}");
                return LiteralText.Of(tag.ToString());
            }
        }
    }
}