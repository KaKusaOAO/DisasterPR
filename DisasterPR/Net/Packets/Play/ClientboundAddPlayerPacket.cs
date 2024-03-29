using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public PlayerDataModel Player { get; set; }

    public ClientboundAddPlayerPacket(IPlayer player)
    {
        Player = PlayerDataModel.FromPlayer(player);
    }

    public ClientboundAddPlayerPacket(BufferReader stream)
    {
        Player = stream.ReadPlayerModel();
    }
    
    public ClientboundAddPlayerPacket(JsonObject payload)
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

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleAddPlayer(this);
}