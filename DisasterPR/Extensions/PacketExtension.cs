using DisasterPR.Net.Packets;
using Mochi.IO;
using Mochi.Structs;

namespace DisasterPR.Extensions;

public static class PacketExtension
{
    public static DataSize CalculatePacketSize(this IPacket packet)
    {
        var buffer = new MemoryStream();
        var writer = new BufferWriter(buffer);
        packet.Write(writer);
        return new DataSize(buffer.Position);
    }
}