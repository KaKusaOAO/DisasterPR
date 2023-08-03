using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundHostRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundHostRoomPacket()
    {
        
    }
    
    public ServerboundHostRoomPacket(PacketContent content)
    {
        
    } 
    
    public void Write(BufferWriter stream)
    {
        
    }

    public void Write(JsonObject obj)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleHostRoom(this);
}