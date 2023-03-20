using System.Collections.Concurrent;
using DisasterPR.Net.Packets;

namespace DisasterPR.Net;

public class PacketStream : IDisposable
{
    public AbstractPlayerConnection Connection { get; }

    private ConcurrentQueue<IPacket> _queue = new();

    private bool _stopped;
    
    public PacketStream(AbstractPlayerConnection connection)
    {
        Connection = connection;
        
        Connection.ReceivedPacket += async e =>
        {
            await Task.Yield();
            _queue.Enqueue(e.Packet);
        };
    }

    public async Task<IPacket> GetNextPacketAsync()
    {
        while (!_stopped)
        {
            await Task.Yield();
            SpinWait.SpinUntil(() => !_queue.IsEmpty || !Connection.IsConnected);

            if (!Connection.IsConnected)
            {
                throw new Exception("WebSocket disconnected");
            }
            
            if (_queue.TryDequeue(out var packet))
            {
                return packet;
            }
        }

        throw new ObjectDisposedException(nameof(PacketStream));
    }

    public void Dispose()
    {
        _stopped = true;
    }
}