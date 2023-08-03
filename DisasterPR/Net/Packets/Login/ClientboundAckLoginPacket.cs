using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ClientboundAckLoginPacket : IPacket<IClientLoginPacketHandler>
{
    public PlayerDataModel Player { get; set; }
    
    public ClientboundAckLoginPacket(IPlayer player)
    {
        Player = PlayerDataModel.FromPlayer(player);
    }
    
    public ClientboundAckLoginPacket(PacketContent content)
    {
        if (content.Type == PacketContentType.Binary)
        {
            var stream = content.GetAsBufferReader();
            Player = stream.ReadPlayerModel();
        }
        else // if (content.Type == PacketContentType.Json)
        {
            var obj = content.GetAsJsonObject();
            Player = PlayerDataModel.Deserialize(obj["player"]!.AsObject());
        }
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WritePlayerModel(Player);
    }

    public void Write(JsonObject obj)
    {
        obj["player"] = Player.SerializeToJson();
    }

    public void Handle(IClientLoginPacketHandler handler) => handler.HandleAckLogin(this);
}