using System.Buffers.Text;
using System.Text.Json.Nodes;

namespace DisasterPR.Net.Packets;

public class PlayerDataModel
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public byte[]? AvatarData { get; set; }
    public string Identifier { get; set; }

    public static PlayerDataModel Deserialize(JsonNode? node)
    {
        var obj = (node as JsonObject)!;
        byte[]? avatar = null;
        if (obj.TryGetPropertyValue("avatar", out var avatarNode))
        {
            avatar = Convert.FromBase64String(avatarNode!.GetValue<string>());
        }

        return new PlayerDataModel
        {
            Guid = Guid.Parse(obj["guid"]!.GetValue<string>()),
            Name = obj["name"]!.GetValue<string>(),
            AvatarData = avatar,
            Identifier = obj["identifier"]!.GetValue<string>()
        };
    }

    public JsonObject SerializeToJson()
    {
        return new JsonObject
        {
            ["guid"] = Guid.ToString(),
            ["name"] = Name,
            ["avatar"] = AvatarData == null ? null : Convert.ToBase64String(AvatarData),
            ["identifier"] = Identifier
        };
    }

    public static PlayerDataModel FromPlayer(IPlayer player) =>
        new()
        {
            Guid = player.Id,
            Name = player.Name,
            Identifier = player.Identifier,
            AvatarData = player.AvatarData
        };
}