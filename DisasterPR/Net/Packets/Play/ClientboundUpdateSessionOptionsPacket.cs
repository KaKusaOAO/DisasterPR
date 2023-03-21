using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateSessionOptionsPacket : IPacket<IClientPlayPacketHandler> 
{
    public SessionOptions Options { get; set; }
    
    public ClientboundUpdateSessionOptionsPacket(SessionOptions options)
    {
        Options = options;
    }

    public ClientboundUpdateSessionOptionsPacket(MemoryStream stream)
    {
        Options = SessionOptions.Deserialize(stream);
    }

    public void Write(MemoryStream stream)
    {
        Options.Serialize(stream);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleUpdateSessionOptionsPacket(this);
}