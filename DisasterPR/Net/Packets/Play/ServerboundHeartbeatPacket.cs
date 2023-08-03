using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundHeartbeatPacket : IPacketNoContent<IServerPlayPacketHandler>
{
    public ServerboundHeartbeatPacket()
    {
        
    }
    
    public ServerboundHeartbeatPacket(PacketContent content)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}