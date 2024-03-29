using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using DisasterPR.Net.Packets;
using Mochi.IO;
using Mochi.Utils;

namespace DisasterPR.Net;

public class RawPacketIO
{
    public WebSocket WebSocket { get; }
    public PacketContentType ContentType { get; }
    private MemoryStream _buffer = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _readLock = new(1, 1);

    public RawPacketIO(WebSocket webSocket, PacketContentType contentType = PacketContentType.Binary)
    {
        WebSocket = webSocket;
        ContentType = contentType;
    }

    public async Task<List<BufferReader>> ReadRawPacketsAsync(CancellationToken token)
    {
        try
        {
            await _readLock.WaitAsync(token);
            var buffer = new byte[4096];
            var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
            _buffer.Write(buffer, 0, result.Count);
            
            if (WebSocket.CloseStatus.HasValue)
            {
                Logger.Info("CloseStatus value presents");
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
            }

            var list = new List<BufferReader>();
            if (ContentType == PacketContentType.Binary || result.EndOfMessage)
            {
                while (CanReadPacket())
                {
                    list.Add(new BufferReader(ReadRawPacket()));
                }
            }

            return list;
        }
        finally
        {
            _readLock.Release();
        }
    }

    public async Task SendRawPacketAsync(MemoryStream stream, CancellationToken token)
    {
        await Common.AcquireSemaphoreAsync(_writeLock, async () =>
        {
            var len = (int) stream.Position;
            var buffer = stream.GetBuffer();
            var buf = new MemoryStream();
            var writer = new BufferWriter(buf);
            if (ContentType == PacketContentType.Binary)
            {
                writer.WriteVarInt(len);
            }

            buf.Write(buffer, 0, len);

            if (WebSocket.CloseStatus.HasValue) return;

            var arr = new byte[buf.Position];
            Array.Copy(buf.GetBuffer(), 0, arr, 0, arr.Length);
            await WebSocket.SendAsync(new ArraySegment<byte>(arr), ContentType == PacketContentType.Binary ? 
                    WebSocketMessageType.Binary : WebSocketMessageType.Text, true,
                token);
        });
    }

    public async Task SendPacketAsync(ConnectionProtocol protocol, PacketFlow flow, IPacket packet, CancellationToken token)
    {
        var buffer = new MemoryStream();
        var id = protocol.GetPacketId(flow, packet);

        if (ContentType == PacketContentType.Binary)
        {
            var writer = new BufferWriter(buffer);
            writer.WriteVarInt(id);
            packet.Write(writer);
        } else if (ContentType == PacketContentType.Json)
        {
            var obj = new JsonObject();
            obj["op"] = id;

            var data = new JsonObject();
            packet.Write(data);
            obj["d"] = data;
            
            await JsonSerializer.SerializeAsync(buffer, obj, cancellationToken: token);
        }

        await SendRawPacketAsync(buffer, token);
    }

    private bool CanReadPacket()
    {
        var total = _buffer.Position;
        if (total == 0) return false;
        if (ContentType == PacketContentType.Json) return true;

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
        var total = _buffer.Position;
        
        // Reset the cursor to 0
        _buffer.Position = 0;
        
        // Read the packet length
        var len = ContentType == PacketContentType.Json ? (int) total : reader.ReadVarInt();

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