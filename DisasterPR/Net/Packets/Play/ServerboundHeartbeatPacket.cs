using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundHeartbeatPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundHeartbeatPacket()
    {
        
    }
    
    public ServerboundHeartbeatPacket(BufferReader stream)
    {
        
    }
    
    public void Write(BufferWriter stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}