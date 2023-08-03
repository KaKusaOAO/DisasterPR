using System.Text.Json.Nodes;
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
    
    public ClientboundSetCardPackPacket(JsonObject payload)
    {
        var data = payload["pack"]!.AsObject();
        CardPack = CardPack.Deserialize(data);
    }
    
    public void Write(BufferWriter stream)
    {
        CardPack.Serialize(stream);
    }

    public void Write(JsonObject obj)
    {
        obj["pack"] = CardPack.SerializeToJson();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetCardPack(this);
}