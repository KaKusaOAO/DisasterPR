namespace DisasterPR.Net;

public enum PacketFlow
{
    Clientbound,
    Serverbound
}

public static class PacketFlowExtension
{
    public static PacketFlow Opposite(this PacketFlow flow) => flow switch
    {
        PacketFlow.Clientbound => PacketFlow.Serverbound,
        PacketFlow.Serverbound => PacketFlow.Clientbound,
        _ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
    };
}