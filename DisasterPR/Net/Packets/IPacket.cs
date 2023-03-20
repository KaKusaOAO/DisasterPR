namespace DisasterPR.Net.Packets;

public interface IPacket
{
    public void Write(MemoryStream stream);
    public Task HandleAsync(IPacketHandler handler);
}

public interface IPacket<in T> : IPacket where T: IPacketHandler
{
    public Task HandleAsync(T handler);

    Task IPacket.HandleAsync(IPacketHandler handler) => HandleAsync((T)handler);
}