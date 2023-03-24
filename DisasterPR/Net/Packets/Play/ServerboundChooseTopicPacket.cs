using DisasterPR.Extensions;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChooseTopicPacket : IPacket<IServerPlayPacketHandler>
{
    public HorizontalSide Side { get; set; }

    public ServerboundChooseTopicPacket(HorizontalSide side)
    {
        Side = side;
    }

    public ServerboundChooseTopicPacket(MemoryStream stream)
    {
        Side = (HorizontalSide) stream.ReadVarInt();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt((int) Side);
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleChooseTopic(this);
}