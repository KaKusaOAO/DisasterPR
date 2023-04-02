using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerScorePacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    public int Score { get; set; }

    public ClientboundUpdatePlayerScorePacket(IPlayer player, int score)
    {
        PlayerId = player.Id;
        Score = score;
    }

    public ClientboundUpdatePlayerScorePacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
        Score = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);
        stream.WriteVarInt(Score);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerScore(this);
}