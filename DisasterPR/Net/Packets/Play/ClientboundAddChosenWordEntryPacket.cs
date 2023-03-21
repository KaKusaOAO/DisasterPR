using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public List<int> Words { get; set; }

    public ClientboundAddChosenWordEntryPacket(Guid id, Guid playerId, List<int> words)
    {
        Id = id;
        PlayerId = playerId;
        Words = words;
    }

    public ClientboundAddChosenWordEntryPacket(MemoryStream stream)
    {
        Id = stream.ReadGuid();
        PlayerId = stream.ReadGuid();
        Words = stream.ReadList(s => s.ReadVarInt());
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteGuid(Id);
        stream.WriteGuid(PlayerId);
        stream.WriteList(Words, (s, i) => s.WriteVarInt(i));
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleAddChosenWordEntryAsync(this);
}