using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundReplacePlayerPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; }

    public ClientboundReplacePlayerPacket(int index, IPlayer player)
    {
        Index = index;
        PlayerId = player.Id;
        PlayerName = player.Name;
    }

    public ClientboundReplacePlayerPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
        PlayerId = stream.ReadGuid();
        PlayerName = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
        stream.WriteGuid(PlayerId);
        stream.WriteUtf8String(PlayerName);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleReplacePlayer(this);
}