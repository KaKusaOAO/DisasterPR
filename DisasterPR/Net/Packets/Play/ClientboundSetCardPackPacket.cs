using DisasterPR.Cards;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetCardPackPacket : IPacket<IClientPlayPacketHandler>
{
    public CardPack CardPack { get; set; }

    public ClientboundSetCardPackPacket(CardPack pack)
    {
        CardPack = pack;
    }

    public ClientboundSetCardPackPacket(MemoryStream stream)
    {
        CardPack = CardPack.Deserialize(stream);
    }
    
    public void Write(MemoryStream stream)
    {
        CardPack.Serialize(stream);
    }

    public Task HandleAsync(IClientPlayPacketHandler handler) => handler.HandleSetCardPackAsync(this);
}