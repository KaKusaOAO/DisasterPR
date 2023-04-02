using DisasterPR.Cards;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetCandidateTopicsPacket : IPacket<IClientPlayPacketHandler>
{
    public int Left { get; set; }
    public int Right { get; set; }

    public ClientboundSetCandidateTopicsPacket(int left, int right)
    {
        Left = left;
        Right = right;
    }

    public ClientboundSetCandidateTopicsPacket(BufferReader stream)
    {
        Left = stream.ReadVarInt();
        Right = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Left);
        stream.WriteVarInt(Right);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetCandidateTopics(this);
}