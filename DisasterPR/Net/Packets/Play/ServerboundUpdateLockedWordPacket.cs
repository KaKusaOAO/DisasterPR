using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundUpdateLockedWordPacket : IPacket<IServerPlayPacketHandler>
{
    public int Index { get; set; }
    public bool IsLocked { get; set; }
    
    public ServerboundUpdateLockedWordPacket(int index, bool locked)
    {
        Index = index;
        IsLocked = locked;
    }

    public ServerboundUpdateLockedWordPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
        IsLocked = stream.ReadBool();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
        stream.WriteBool(IsLocked);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
        obj["locked"] = IsLocked;
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleUpdateLockedWord(this);
}