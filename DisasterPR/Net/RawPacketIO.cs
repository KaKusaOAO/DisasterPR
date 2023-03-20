using System.Net.WebSockets;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets;

namespace DisasterPR.Net;

public class RawPacketIO
{
    public WebSocket WebSocket { get; }

    private MemoryStream _buffer = new();

    public RawPacketIO(WebSocket webSocket)
    {
        WebSocket = webSocket;
    }

    public async Task<List<MemoryStream>> ReadRawPacketsAsync(CancellationToken token)
    {
        var buffer = new byte[4096];
        var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
        _buffer.Write(buffer, 0, result.Count);

        var list = new List<MemoryStream>();
        while (CanReadPacket())
        {
            list.Add(ReadRawPacket());
        }

        return list;
    }

    public async Task<List<IPacket>> ReadPacketsAsync(ConnectionProtocol protocol, PacketFlow flow, CancellationToken token)
    {
        var streams = await ReadRawPacketsAsync(token);
        return streams.Select(stream =>
        {
            var id = stream.ReadVarInt();
            return protocol.CreatePacket(flow, id, stream);
        }).ToList();
    }

    public async Task SendRawPacketAsync(MemoryStream stream, CancellationToken token)
    {
        var buffer = stream.GetBuffer();
        var buf = new MemoryStream();
        buf.WriteVarInt(buffer.Length);
        buf.Write(buffer, 0, buffer.Length);
        
        await WebSocket.SendAsync(new ArraySegment<byte>(buf.GetBuffer()), WebSocketMessageType.Binary, false, token);
    }

    public async Task SendPacketAsync(ConnectionProtocol protocol, PacketFlow flow, IPacket packet, CancellationToken token)
    {
        var buffer = new MemoryStream();
        var id = protocol.GetPacketId(flow, packet);
        buffer.WriteVarInt(id);
        packet.Write(buffer);

        await SendRawPacketAsync(buffer, token);
    }

    private bool CanReadPacket()
    {
        var pos = _buffer.Position;
        var len = _buffer.ReadVarInt();
        var result = _buffer.Length - _buffer.Position >= len;

        _buffer.Position = pos;
        return result;
    }

    public MemoryStream ReadRawPacket()
    {
        var len = _buffer.ReadVarInt();

        var buffer = new byte[len];
        _buffer.Read(buffer, 0, len);

        var newBuffer = new MemoryStream();
        _buffer.CopyTo(newBuffer);
        newBuffer.Position = 0;
        _buffer = newBuffer;
        
        return new MemoryStream(buffer);
    }
}