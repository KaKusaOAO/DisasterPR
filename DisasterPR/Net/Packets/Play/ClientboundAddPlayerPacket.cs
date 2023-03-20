namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public void Write(MemoryStream stream)
    {
        // throw new NotImplementedException();
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleAddPlayerAsync(this);
}