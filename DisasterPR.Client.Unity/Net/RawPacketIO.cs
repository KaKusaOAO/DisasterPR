using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Extensions;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using Mochi.IO;

namespace DisasterPR.Client.Unity.Net;

public class RawPacketIO
{
    public IWebSocket WebSocket { get; }
    private MemoryStream _buffer = new();
    private bool _closed;
    private WebSocketCloseCode _closeCode;

    public bool IsClosed => _closed;
    public WebSocketCloseCode? CloseCode => _closed ? null : _closeCode;
    
    public event Action<List<MemoryStream>> OnPacketReceived;

    public RawPacketIO(IWebSocket webSocket)
    {
        WebSocket = webSocket;
        WebSocket.OnMessage += WebSocketOnOnMessage;
        WebSocket.OnClose += e =>
        {
            _closed = true;
            _closeCode = e;
        };
    }

    private void WebSocketOnOnMessage(byte[] data)
    {
        _buffer.Write(data, 0, data.Length);

        var list = new List<MemoryStream>();
        while (CanReadPacket())
        {
            list.Add(ReadRawPacket());
        }
        
        OnPacketReceived?.Invoke(list);
    }

    public void SendRawPacket(MemoryStream stream)
    {
        var len = (int) stream.Position;
        var buffer = stream.GetBuffer();
        var buf = new MemoryStream();
        var writer = new BufferWriter(buf);
        writer.WriteVarInt(len);
        buf.Write(buffer, 0, len);

        if (_closed) return;

        var arr = new byte[buf.Position];
        Array.Copy(buf.GetBuffer(), 0, arr, 0, arr.Length);
        WebSocket.Send(arr);
    }

    public void SendPacket(ConnectionProtocol protocol, PacketFlow flow, IPacket packet)
    {
        var buffer = new MemoryStream();
        var writer = new BufferWriter(buffer);
        var id = protocol.GetPacketId(flow, packet);
        writer.WriteVarInt(id);
        packet.Write(writer);

        SendRawPacket(buffer);
    }

    private bool CanReadPacket()
    {
        var total = _buffer.Position;
        if (total == 0) return false;

        var reader = new BufferReader(_buffer);
        _buffer.Position = 0;
        
        var len = reader.ReadVarInt();
        if (_buffer.Position >= total) return false;
        
        var result = total >= len;
        _buffer.Position = total;
        return result;
    }

    public MemoryStream ReadRawPacket()
    {
        var reader = new BufferReader(_buffer);
        
        // Reset the cursor to 0
        _buffer.Position = 0;
        
        // Read the packet length
        var len = reader.ReadVarInt();

        // Read the packet content
        var buffer = new byte[len];
        _buffer.Read(buffer, 0, len);

        // Write remaining to a new stream
        var newBuffer = new MemoryStream();
        _buffer.CopyTo(newBuffer);
        
        // Set our buffer to the new buffer
        newBuffer.Position = 0;
        _buffer = newBuffer;
        
        return new MemoryStream(buffer);
    }
}