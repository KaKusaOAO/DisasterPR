using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundJoinedRoomPacket : IPacket<IClientPlayPacketHandler>
{
    public int RoomId { get; set; }
    
    // All players without the receiver
    public List<AddPlayerEntry> Players { get; set; }
    
    public ClientboundJoinedRoomPacket(ISession session)
    {
        RoomId = session.RoomId;
        Players = session.Players.Select(p => new AddPlayerEntry
        {
            Guid = p.Id,
            Name = p.Name
        }).ToList();
    }

    public ClientboundJoinedRoomPacket(MemoryStream stream)
    {
        RoomId = stream.ReadVarInt();
        Players = stream.ReadList(s => s.ReadAddPlayerEntry());
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(RoomId);
        stream.WriteList(Players, (s, p) => s.WriteAddPlayerEntry(p));
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleJoinedRoomAsync(this);
}