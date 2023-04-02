using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddChosenWordEntryPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Id { get; set; }
    public Guid? PlayerId { get; set; }
    public List<int> Words { get; set; }

    public ClientboundAddChosenWordEntryPacket(Guid id, Guid? playerId, List<int> words)
    {
        Id = id;
        PlayerId = playerId;
        Words = words;
    }

    public ClientboundAddChosenWordEntryPacket(BufferReader stream)
    {
        Id = stream.ReadGuid();
        if (stream.ReadBool())
        {
            PlayerId = stream.ReadGuid();
        }

        Words = stream.ReadList(s => s.ReadVarInt());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
        stream.WriteBool(PlayerId.HasValue);
        if (PlayerId.HasValue)
        {
            stream.WriteGuid(PlayerId.Value);
        }

        stream.WriteList(Words, (s, i) => s.WriteVarInt(i));
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleAddChosenWordEntry(this);
}