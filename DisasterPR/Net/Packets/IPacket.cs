using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets;

public interface IPacket
{
    public void Write(BufferWriter writer);
    public void Write(JsonObject obj);
    public void Handle(IPacketHandler handler);
}

public interface IPacket<in T> : IPacket where T: IPacketHandler
{
    public void Handle(T handler);

    void IPacket.Handle(IPacketHandler handler) => Handle((T)handler);
}

public interface IPacketNoContent<in T> : IPacket<T> where T : IPacketHandler
{
    void IPacket.Write(BufferWriter writer) {}
    void IPacket.Write(JsonObject obj) {}
}