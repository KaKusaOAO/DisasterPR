using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateLockedWordPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    public bool IsLocked { get; set; }
    
    public ClientboundUpdateLockedWordPacket(int index, bool locked)
    {
        Index = index;
        IsLocked = locked;
    }

    public ClientboundUpdateLockedWordPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
        IsLocked = stream.ReadBool();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
        stream.WriteBool(IsLocked);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateLockedWord(this);
}