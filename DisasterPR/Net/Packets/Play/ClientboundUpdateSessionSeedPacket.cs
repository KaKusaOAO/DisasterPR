using System.Text.Json.Nodes;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdateSessionSeedPacket : IPacket<IClientPlayPacketHandler>
{
    public int Seed { get; set; }

    public ClientboundUpdateSessionSeedPacket(ISession session)
    {
        Seed = session.RandomSeed;
    }

    public ClientboundUpdateSessionSeedPacket(int seed)
    {
        Seed = seed;
    }

    public ClientboundUpdateSessionSeedPacket(BufferReader stream)
    {
        Seed = stream.ReadVarInt();
    }

    public ClientboundUpdateSessionSeedPacket(JsonObject payload)
    {
        Seed = payload["seed"]!.GetValue<int>();
    }

    public void Write(BufferWriter writer)
    {
        writer.WriteVarInt(Seed);
    }

    public void Write(JsonObject obj)
    {
        obj["seed"] = Seed;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdateSessionSeed(this);
}