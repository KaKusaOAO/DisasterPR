using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ServerboundChooseFinalPacket : IPacket<IServerPlayPacketHandler>
{
    public int Index { get; set; }

    public ServerboundChooseFinalPacket(int index)
    {
        Index = index;
    }

    public ServerboundChooseFinalPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
    }

    public void Handle(IServerPlayPacketHandler handler) => handler.HandleChooseFinal(this);
}