using DisasterPR.Extensions;

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

    public ClientboundUpdateLockedWordPacket(MemoryStream stream)
    {
        Index = stream.ReadVarInt();
        IsLocked = stream.ReadBool();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(Index);
        stream.WriteBool(IsLocked);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateLockedWord(this);
}