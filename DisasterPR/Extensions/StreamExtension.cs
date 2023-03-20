using System.Text;

namespace DisasterPR.Extensions;

internal static class StreamExtension
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
            b = (byte) stream.ReadByte();
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
            b = (byte) stream.ReadByte();
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
}