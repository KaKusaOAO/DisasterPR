using DisasterPR.Net.Packets;
using Mochi.IO;
using Mochi.Utils;

namespace DisasterPR.Extensions;

public static class StreamExtension
{
    public static PlayerDataModel ReadPlayerModel(this BufferReader reader) =>
        new()
        {
            Guid = reader.ReadGuid(),
            Name = reader.ReadUtf8String(),
            Identifier = reader.ReadUtf8String(),
            AvatarData = reader.ReadOptional(r => r.ReadByteArray()).OrElse((byte[]?) null)
        };

    public static void WritePlayerModel(this BufferWriter writer, PlayerDataModel model)
    {
        writer.WriteGuid(model.Guid);
        writer.WriteUtf8String(model.Name);
        writer.WriteUtf8String(model.Identifier);

        var hasAvatar = model.AvatarData != null;
        writer.WriteBool(hasAvatar);
        if (hasAvatar) writer.WriteByteArray(model.AvatarData!);
    }
}