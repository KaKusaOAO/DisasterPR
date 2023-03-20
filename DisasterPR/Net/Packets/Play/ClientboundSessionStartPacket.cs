namespace DisasterPR.Net.Packets.Play;

public class ClientboundSessionStartPacket : IPacket<IClientPlayPacketHandler>
{
    public void Write(MemoryStream stream)
    {
        // throw new NotImplementedException();
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleSessionStartAsync(this);
}