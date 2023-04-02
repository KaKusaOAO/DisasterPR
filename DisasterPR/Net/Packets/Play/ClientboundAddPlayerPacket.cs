using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundAddPlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; }

    public ClientboundAddPlayerPacket(IPlayer player)
    {
        PlayerId = player.Id;
        PlayerName = player.Name;
    }

    public ClientboundAddPlayerPacket(BufferReader stream)
    {
        PlayerId = stream.ReadGuid();
        PlayerName = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(PlayerId);
        stream.WriteUtf8String(PlayerName);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleAddPlayer(this);
}