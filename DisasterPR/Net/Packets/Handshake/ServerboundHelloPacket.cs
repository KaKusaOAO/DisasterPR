using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Handshake;

public class ServerboundHelloPacket : IPacket<IServerHandshakePacketHandler>
{
    public int Version { get; set; }

    public ServerboundHelloPacket(int version)
    {
        Version = version;
    }
    
    public ServerboundHelloPacket(MemoryStream stream)
    {
        Version = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Version);
    }

    public Task HandleAsync(IServerHandshakePacketHandler handler) => handler.HandleHelloAsync(this);
}