using System.Text.Json.Nodes;
using Mochi.IO;

namespace DisasterPR.Net.Packets;

public class PacketSet
{
    private Dictionary<int, Type> _packetMap = new();
    private Dictionary<Type, int> _packetIdMap = new();
    private Dictionary<int, Func<PacketContent, IPacket>> _deserializers = new();

    /// <summary>
    /// Adds a packet to the set. <br/>
    /// The packet type must have a constructor that takes a <see cref="BufferReader"/> as its only parameter. <br/>
    /// Note that in Unity, managed code stripping might remove the constructor, which will cause this method to fail.
    /// </summary>
    /// <typeparam name="T">The type of the packet.</typeparam>
    /// <returns>This packet set for chain call.</returns>
    public PacketSet AddPacket<T>() where T : IPacket => AddPacket(typeof(T));

    /// <summary>
    /// Adds a packet to the set and specifies a custom deserializer. <br/>
    /// This resolves the issue of managed code stripping in Unity removing the constructor of the packet type.
    /// </summary>
    /// <param name="deserializer">The deserializer of the packet type.</param>
    /// <typeparam name="T">The packet type.</typeparam>
    /// <returns>This packet set for chain call.</returns>
    public PacketSet AddPacket<T>(Func<PacketContent, T> deserializer) where T : IPacket
    {
        AddPacket<T>();
        _deserializers.Add(_packetIdMap[typeof(T)], s => deserializer(s));
        return this;
    }

    public PacketSet AddPacket<T>(Func<BufferReader, T> binary, Func<JsonObject, T> json) where T : IPacket =>
        AddPacket<T>(content =>
        {
            switch (content.Type)
            {
                case PacketContentType.Binary:
                {
                    var stream = content.GetAsBufferReader();
                    return binary(stream);
                }
                case PacketContentType.Json:
                {
                    var payload = content.GetAsJsonObject();
                    return json(payload);
                }
                default:
                    throw new Exception("Unknown content type");
            }
        });

    /// <summary>
    /// Adds a packet to the set. <br/>
    /// The packet type must have a constructor that takes a <see cref="MemoryStream"/> as its only parameter. <br/>
    /// Note that in Unity, managed code stripping might remove the constructor, which will cause this method to fail.
    /// </summary>
    /// <returns>This packet set for chain call.</returns>
    public PacketSet AddPacket(Type type)
    {
        if (!typeof(IPacket).IsAssignableFrom(type))
        {
            throw new ArgumentException($"{type} is not a Packet type", nameof(type));
        }

        var id = _packetMap.Count;
        _packetMap.Add(id, type);
        _packetIdMap.Add(type, id);
        return this;
    }

    public Type GetPacketTypeById(int id)
    {
        if (!_packetMap.ContainsKey(id))
        {
            throw new ArgumentException($"Bad packet ID {id}");
        }
        
        return _packetMap[id];
    }
    
    public Func<PacketContent, IPacket> GetDeserializerById(int id)
    {
        if (_deserializers.ContainsKey(id))
        {
            return _deserializers[id];
        }
        
        var type = GetPacketTypeById(id);
        var ctor = type.GetConstructor(new[] { typeof(BufferReader) });
        if (ctor == null)
        {
            throw new NotSupportedException($"The MemoryStream constructor in type {type} is not defined");
        }

        return stream => (IPacket) ctor.Invoke(new[] { stream });
    }

    public int GetPacketId(IPacket packet)
    {
        var type = packet.GetType();
        if (!_packetIdMap.ContainsKey(type))
        {
            throw new ArgumentException($"Packet type {type} not registered");
        }

        return _packetIdMap[type];
    }

    public IPacket CreatePacket(int id, BufferReader stream) => 
        GetDeserializerById(id)(new PacketContent(stream));
    
    public IPacket CreatePacket(int id, JsonObject obj) => 
        GetDeserializerById(id)(new PacketContent(obj));

    public T CreatePacket<T>(int id, BufferReader stream) where T : IPacket =>
        (T) CreatePacket(id, stream);
    
    public T CreatePacket<T>(int id, JsonObject stream) where T : IPacket =>
        (T) CreatePacket(id, stream);
}