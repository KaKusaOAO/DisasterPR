using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundJoinRoomPacket : IPacket<IServerPlayPacketHandler>
{
    public int RoomId { get; set; }

    public ServerboundJoinRoomPacket(int roomId)
    {
        RoomId = roomId;
    }

    public ServerboundJoinRoomPacket(BufferReader stream)
    {
        RoomId = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(RoomId);
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleJoinRoom(this);
}