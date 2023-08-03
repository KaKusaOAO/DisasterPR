using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundReplacePlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    public PlayerDataModel Player { get; set; }

    public ClientboundReplacePlayerPacket(int index, IPlayer player)
    {
        Index = index;
        Player = PlayerDataModel.FromPlayer(player);
    }

    public ClientboundReplacePlayerPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
        Player = stream.ReadPlayerModel();
    }
    
    public ClientboundReplacePlayerPacket(JsonObject payload)
    {
        Index = payload["index"]!.GetValue<int>();
        Player = PlayerDataModel.Deserialize(payload["player"]!.AsObject());
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
        stream.WritePlayerModel(Player);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
        obj["player"] = Player.SerializeToJson();
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleReplacePlayer(this);
}