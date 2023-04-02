using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundHeartbeatPacket : IPacket<IClientPlayPacketHandler>
{
    public ClientboundHeartbeatPacket()
    {
        
    }
    
    public ClientboundHeartbeatPacket(BufferReader stream)
    {
        
    }
    
    public void Write(BufferWriter stream)
    {
        
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}