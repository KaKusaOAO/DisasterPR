using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerDataPacket : IPacket<IClientPlayPacketHandler>
{
    public PlayerDataModel Player { get; set; }

    public ClientboundUpdatePlayerDataPacket(IPlayer player)
    {
        Player = PlayerDataModel.FromPlayer(player);
    }

    public ClientboundUpdatePlayerDataPacket(BufferReader stream)
    {
        Player = stream.ReadPlayerModel();
    }

    public ClientboundUpdatePlayerDataPacket(JsonObject payload)
    {
        Player = PlayerDataModel.Deserialize(payload["player"]!.AsObject());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WritePlayerModel(Player);
    }

    public void Write(JsonObject obj)
    {
        obj["player"] = Player.SerializeToJson();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerData(this);
}