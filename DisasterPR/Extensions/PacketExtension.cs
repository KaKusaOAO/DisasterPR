using DisasterPR.Net.Packets;
using KaLib.Structs;

namespace DisasterPR.Extensions;

public static class PacketExtension
{
    public static DataSize CalculatePacketSize(this IPacket packet)
    {
        var buffer = new MemoryStream();
        packet.Write(buffer);
        return new DataSize(buffer.Position);
    }
}