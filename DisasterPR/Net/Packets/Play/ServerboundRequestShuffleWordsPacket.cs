using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestShuffleWordsPacket : IPacketNoContent<IServerPlayPacketHandler>
{

    public ServerboundRequestShuffleWordsPacket()
    {
    }

    public ServerboundRequestShuffleWordsPacket(BufferReader stream)
    {
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestShuffleWords(this);
}