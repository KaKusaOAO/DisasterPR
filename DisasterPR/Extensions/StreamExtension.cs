using DisasterPR.Net.Packets;
using Mochi.IO;

namespace DisasterPR.Extensions;

public static class StreamExtension
{
    public static AddPlayerEntry ReadAddPlayerEntry(this BufferReader reader) =>
        new()
        {
            Guid = reader.ReadGuid(),
            Name = reader.ReadUtf8String()
        };

    public static void WriteAddPlayerEntry(this BufferWriter writer, AddPlayerEntry entry)
    {
        writer.WriteGuid(entry.Guid);
        writer.WriteUtf8String(entry.Name);
    }
}