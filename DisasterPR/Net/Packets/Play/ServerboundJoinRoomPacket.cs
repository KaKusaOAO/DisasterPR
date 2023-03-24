using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundJoinRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public int RoomId { get; set; }

    public ServerboundJoinRoomPacket(int roomId)
    {
        RoomId = roomId;
    }

    public ServerboundJoinRoomPacket(MemoryStream stream)
    {
        RoomId = stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(RoomId);
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleJoinRoom(this);
}