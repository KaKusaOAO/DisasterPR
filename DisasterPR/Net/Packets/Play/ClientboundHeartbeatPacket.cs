using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundHeartbeatPacket : IPacketNoContent<IClientPlayPacketHandler>
{
    public ClientboundHeartbeatPacket()
    {
        
    }
    
    public ClientboundHeartbeatPacket(PacketContent stream)
    {
        
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleHeartbeat(this);
}