namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestRoomStartPacket : IPacket<IServerPlayPacketHandler>
{
    public ServerboundRequestRoomStartPacket()
    {
        
    }

    public ServerboundRequestRoomStartPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestRoomStart(this);
}