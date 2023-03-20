using DisasterPR.Net.Packets;

namespace DisasterPR.Events;

public class ReceivedPacketEventArgs : EventArgs
{
    public IPacket Packet { get; set; }
}