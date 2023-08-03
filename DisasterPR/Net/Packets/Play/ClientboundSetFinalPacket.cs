using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetFinalPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }

    public ClientboundSetFinalPacket(int index)
    {
        Index = index;
    }

    public ClientboundSetFinalPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
    }

    public ClientboundSetFinalPacket(JsonObject payload)
    {
        Index = payload["index"]!.GetValue<int>();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetFinal(this);
}