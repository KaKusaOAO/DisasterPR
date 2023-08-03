using DisasterPR.Net.Packets;
using Mochi.IO;
using Mochi.Utils;

namespace DisasterPR.Extensions;

public static class StreamExtension
{
    public static AddPlayerEntry ReadAddPlayerEntry(this BufferReader reader) =>
        new()
        {
            Guid = reader.ReadGuid(),
            Name = reader.ReadUtf8String(),
            Identifier = reader.ReadUtf8String(),
            AvatarData = reader.ReadOptional(r => r.ReadByteArray()).OrElse((byte[]?) null)
        };

    public static void WriteAddPlayerEntry(this BufferWriter writer, AddPlayerEntry entry)
    {
        writer.WriteGuid(entry.Guid);
        writer.WriteUtf8String(entry.Name);
        writer.WriteUtf8String(entry.Identifier);

        var hasAvatar = entry.AvatarData != null;
        writer.WriteBool(hasAvatar);
        if (hasAvatar) writer.WriteByteArray(entry.AvatarData!);
    }
}