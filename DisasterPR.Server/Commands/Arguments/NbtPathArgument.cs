using System.Text;
using KaLib.Brigadier.Arguments;
using KaLib.Brigadier.Exceptions;
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
        var conditional = false;
        
        while (reader.CanRead())
        {
            var c = reader.Peek();
            if (c == '?')
            {
                if (first) throw new Exception("Unexpected '?'");
                
                reader.Skip();
                conditional = true;
                continue;
            }
            
            if (c == '[')
            {
                reader.Skip();

                if (reader.Peek() == '$')
                {
                    var path = Parse(reader);
                    reader.Expect(']');

                    var so = new SelectOperation(path) as PathOperation;
                    if (conditional) so = new ConditionalOperation(so);
                    operations.Add(so);
                    first = false;
                    continue;
                }
                
                var index = reader.ReadInt();
                reader.Expect(']');

                var op = new IndexOperation(index) as PathOperation;
                if (conditional) op = new ConditionalOperation(op);
                operations.Add(op);
                first = false;
                continue;
            }

            if (!first)
            {
                if (reader.Peek() != '.') break;
                reader.Expect('.');
            }

            if (reader.CanRead() && reader.Peek() == '$')
            {
                reader.Skip();
                operations.Clear();
                first = false;
                continue;
            }

            var name = new StringBuilder();
            if (!reader.CanRead())
            {
                throw new Exception("Expected path");
            }
            
            var r = reader.Peek();
            if (StringReader.IsQuotedStringStart(r))
            {
                reader.Skip();
                var str = reader.ReadStringUntil(r);
                name.Append(str);
                break;
            }
            
            while (reader.CanRead())
            {
                var r2 = reader.Peek();
                if (r2 == '.' || !StringReader.IsAllowedInUnquotedString(r2)) break;
                reader.Skip();
                name.Append(r2);
            }

            if (name.Length == 0)
            {
                throw new Exception("Property name cannot be empty");
            }

            var o = new ChildOperation(name.ToString()) as PathOperation;
            if (conditional) o = new ConditionalOperation(o);
            operations.Add(o);
            first = false;
        }

        return new NbtPath(operations);
    }
}