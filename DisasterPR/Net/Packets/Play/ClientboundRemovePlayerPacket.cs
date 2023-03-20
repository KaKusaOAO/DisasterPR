namespace DisasterPR.Net.Packets.Play;

public class ClientboundRemovePlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public void Write(MemoryStream stream)
    {
        // throw new NotImplementedException();
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleRemovePlayerAsync(this);
}