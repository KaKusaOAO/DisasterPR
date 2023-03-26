using System.Runtime.CompilerServices;
using System.Text;
using DisasterPR.Net.Packets;
using KaLib.Utils;

namespace DisasterPR.Extensions;

public static class vStreamExtension
{
    private const byte SegmentBits = 0x7f;
    private const byte ContinueBit = 0x80;
    
    public static int ReadVarInt(this Stream stream)
    {
        var value = 0;
        var pos = 0;
        byte b;
        
        while (true)
        {
            var read = stream.ReadByte();
            if (read == -1) throw new EndOfStreamException();

            b = (byte) read;
            value |= (b & SegmentBits) << pos;
            if ((b & ContinueBit) == 0) break;

            pos += 7;
            if (pos >= 32)
            {
                throw new Exception("VarInt is too big");
            }
        }

        return value;
    }
    
    public static long ReadVarLong(this Stream stream)
    {
        var value = 0L;
        var pos = 0;
        byte b;
        
        while (true)
        {
            var read = stream.ReadByte();
            if (read == -1) throw new EndOfStreamException();

            b = (byte) read;
            value |= (long) (b & SegmentBits) << pos;
            if ((b & ContinueBit) == 0) break;

            pos += 7;
            if (pos >= 64)
            {
                throw new Exception("VarLong is too big");
            }
        }

        return value;
    }

    public static void WriteVarInt(this Stream stream, int value)
    {
        while (true)
        {
            if ((value & ~SegmentBits) == 0)
            {
                stream.WriteByte((byte) value);
                return;
            }
            
            stream.WriteByte((byte) ((value & SegmentBits) | ContinueBit));
            value >>>= 7;
        }
    }
    
    public static void WriteVarLong(this Stream stream, long value)
    {
        while (true)
        {
            if ((value & ~SegmentBits) == 0)
            {
                stream.WriteByte((byte) value);
                return;
            }
            
            stream.WriteByte((byte) ((value & SegmentBits) | ContinueBit));
            value >>>= 7;
        }
    }

    public static byte[] ReadByteArray(this Stream stream)
    {
        var len = stream.ReadVarInt();
        var arr = new byte[len];
        stream.Read(arr, 0, len);
        return arr;
    }

    public static void WriteByteArray(this Stream stream, byte[] arr)
    {
        stream.WriteVarInt(arr.Length);
        stream.Write(arr, 0, arr.Length);
    }

    public static string ReadString(this Stream stream, Encoding encoding)
    {
        var arr = stream.ReadByteArray();
        return encoding.GetString(arr);
    }

    public static string ReadUtf8String(this Stream stream) => stream.ReadString(Encoding.UTF8);

    public static void WriteString(this Stream stream, string str, Encoding encoding)
    {
        var arr = encoding.GetBytes(str);
        stream.WriteByteArray(arr);
    }

    public static void WriteUtf8String(this Stream stream, string str) => stream.WriteString(str, Encoding.UTF8);

    public static Guid ReadGuid(this Stream stream)
    {
        var arr = new byte[16];
        stream.Read(arr, 0, arr.Length);
        return new Guid(arr);
    }

    public static void WriteGuid(this Stream stream, Guid guid)
    {
        var arr = guid.ToByteArray();
        stream.Write(arr, 0, arr.Length);
    }

    public static List<T> ReadList<T>(this Stream stream, Func<Stream, T> reader)
    {
        var size = stream.ReadVarInt();
        var result = new List<T>();
        for (var i = 0; i < size; i++)
        {
            result.Add(reader(stream));
        }

        return result;
    }
    
    public static void WriteList<T>(this Stream stream, List<T> list, Action<Stream, T> writer)
    {
        stream.WriteVarInt(list.Count);
        foreach (var item in list)
        {
            writer(stream, item);
        }
    }

    public static AddPlayerEntry ReadAddPlayerEntry(this Stream stream) =>
        new()
        {
            Guid = stream.ReadGuid(),
            Name = stream.ReadUtf8String()
        };

    public static void WriteAddPlayerEntry(this Stream stream, AddPlayerEntry entry)
    {
        stream.WriteGuid(entry.Guid);
        stream.WriteUtf8String(entry.Name);
    }

    public static bool ReadBool(this Stream stream)
    {
        var b = stream.ReadByte();
        if (b == -1) throw new EndOfStreamException();
        return b > 0;
    }

    public static void WriteBool(this Stream stream, bool value)
    {
        stream.WriteByte((byte) (value ? 1 : 0));
    }

    public static void WriteOptional<T>(this Stream stream, T? value, Action<Stream, T> writer)
    {
        var present = value == null;
        stream.WriteBool(present);
        if (!present) return;

        writer(stream, value!);
    }
    
    public static void WriteNullable<T>(this Stream stream, T? value, Action<Stream, T> writer) where T : struct
    {
        var present = value.HasValue;
        stream.WriteBool(present);
        if (!present) return;

        writer(stream, value!.Value);
    }
    
    public static T? ReadOptional<T>(this Stream stream, Func<Stream, T> reader) => 
        !stream.ReadBool() ? default : reader(stream);
    
    public static T? ReadNullable<T>(this Stream stream, Func<Stream, T> reader) where T : struct => 
        !stream.ReadBool() ? null : reader(stream);

    public static T ReadEnum<T>(this Stream stream) where T : struct
    {
        var type = Enum.GetUnderlyingType(typeof(T));
        if (type == typeof(int)) return (T)Enum.ToObject(typeof(T), stream.ReadVarInt());
        throw new NotSupportedException();
    }

    public static void WriteEnum<T>(this Stream stream, T val) where T : struct => 
        stream.WriteVarInt((int) Convert.ChangeType(val, TypeCode.Int32));
}