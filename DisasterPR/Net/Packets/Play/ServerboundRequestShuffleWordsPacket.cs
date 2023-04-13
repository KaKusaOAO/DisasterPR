using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundRequestShuffleWordsPacket : IPacket<IServerPlayPacketHandler>
{

    public ServerboundRequestShuffleWordsPacket()
    {
    }

    public ServerboundRequestShuffleWordsPacket(BufferReader stream)
    {
    }
    
    public void Write(BufferWriter stream)
    {
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleRequestShuffleWords(this);
}