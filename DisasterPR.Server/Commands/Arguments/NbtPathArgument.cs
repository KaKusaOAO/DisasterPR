using System.Text;
using KaLib.Brigadier.Arguments;
using StringReader = KaLib.Brigadier.StringReader;

namespace DisasterPR.Server.Commands.Arguments;

public class NbtPathArgument : IArgumentType<NbtPath>
{
    private NbtPathArgument()
    {
        
    }

    public static NbtPathArgument Path() => new();
    
    public NbtPath Parse(StringReader reader)
    {
        var operations = new List<PathOperation>();
        var first = true;
        while (reader.CanRead())
        {
            var c = reader.Peek();
            if (c == '[')
            {
                reader.Skip();
                var index = reader.ReadInt();
                reader.Expect(']');
                operations.Add(new IndexOperation(index));
                first = false;
                continue;
            }

            if (!first) reader.Expect('.');

            var name = new StringBuilder();
            while (reader.CanRead())
            {
                var r = reader.Peek();
                if (StringReader.IsQuotedStringStart(r))
                {
                    reader.Skip();
                    var str = reader.ReadStringUntil(r);
                    name.Append(str);
                    break;
                }
                
                if (r == '.' || !StringReader.IsAllowedInUnquotedString(r)) break;
                reader.Skip();
                name.Append(r);
            }
            
            operations.Add(new ChildOperation(name.ToString()));
            first = false;
        }

        return new NbtPath(operations);
    }
}