namespace DisasterPR.Net.Packets;

public interface INoncePacket : IPacket
{
    public Guid Nonce { get; set; }
}