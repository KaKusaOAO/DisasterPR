using DisasterPR.Cards;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetCardPackPacket : IPacket<IClientPlayPacketHandler>
{
    public CardPack CardPack { get; set; }

    public ClientboundSetCardPackPacket(CardPack pack)
    {
        CardPack = pack;
    }

    public ClientboundSetCardPackPacket(BufferReader stream)
    {
        CardPack = CardPack.Deserialize(stream);
    }
    
    public void Write(BufferWriter stream)
    {
        CardPack.Serialize(stream);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetCardPack(this);
}