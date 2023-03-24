using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetWordsPacket : IPacket<IClientPlayPacketHandler>
{
    public List<int> Words { get; set; }

    public ClientboundSetWordsPacket(List<int> words)
    {
        Words = words;
    }

    public ClientboundSetWordsPacket(MemoryStream stream)
    {
        Words = stream.ReadList(s => s.ReadVarInt());
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteList(Words, (s, i) => s.WriteVarInt(i));
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetWords(this);
}