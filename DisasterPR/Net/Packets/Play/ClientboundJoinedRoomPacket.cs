using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundJoinedRoomPacket : IPacket<IClientPlayPacketHandler>
{
    public int RoomId { get; set; }
    
    public int? SelfIndex { get; set; }
    
    // All players without the receiver
    public List<AddPlayerEntry> Players { get; set; }
    
    public ClientboundJoinedRoomPacket(ISession session, int? selfIndex = null)
    {
        RoomId = session.RoomId;
        SelfIndex = selfIndex;
        Players = session.Players.Select(p => new AddPlayerEntry
        {
            Guid = p.Id,
            Name = p.Name
        }).ToList();
    }

    public ClientboundJoinedRoomPacket(BufferReader stream)
    {
        RoomId = stream.ReadVarInt();
        SelfIndex = stream.ReadNullable(s => s.ReadVarInt());
        Players = stream.ReadList(s => s.ReadAddPlayerEntry());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(RoomId);
        stream.WriteOptional(SelfIndex, (s, v) => s.WriteVarInt(v));
        stream.WriteList(Players, (s, p) => s.WriteAddPlayerEntry(p));
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleJoinedRoom(this);
}