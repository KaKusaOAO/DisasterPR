using System.Reflection;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Net.Packets;

public class ConnectionProtocol
{
    private static readonly Dictionary<PacketState, ConnectionProtocol> _map = new();

    public static readonly ConnectionProtocol Handshake = new(PacketState.Handshake, Protocol()
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundHelloPacket>()
        )
    );

    public static readonly ConnectionProtocol Login = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet())
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundLoginPacket>()
        )
    );
    
    public static readonly ConnectionProtocol Play = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket<ClientboundChatPacket>()
            .AddPacket<ClientboundSessionStartPacket>()
            .AddPacket<ClientboundAddPlayerPacket>()
            .AddPacket<ClientboundRemovePlayerPacket>()
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundChatPacket>()
            .AddPacket<ServerboundHostRoomPacket>()
            .AddPacket<ServerboundJoinRoomPacket>()
        )
    );
    
    public static ConnectionProtocol OfState(PacketState state)
    {
        if (!_map.ContainsKey(state))
        {
            throw new NotSupportedException($"{state} not registered");
        }
        return _map[state];
    }

    public class PacketBuilder
    {
        internal Dictionary<PacketFlow, PacketSet> Flows { get; } = new();

        public PacketBuilder AddFlow(PacketFlow flow, PacketSet packets)
        {
            Flows.Add(flow, packets);
            return this;
        }
    }
    
    public PacketState PacketState { get; }
    private Dictionary<PacketFlow, PacketSet> _flows = new();

    private static PacketBuilder Protocol() => new();
    
    private ConnectionProtocol(PacketState state, PacketBuilder builder)
    {
        if (_map.ContainsKey(state))
        {
            throw new ArgumentException($"{state} already registered");
        }
        
        PacketState = state;
        _flows = builder.Flows;
        _map.Add(state, this);
    }

    public int GetPacketId(PacketFlow flow, IPacket packet) => _flows[flow].GetPacketId(packet);

    public Type GetPacketTypeById(PacketFlow flow, int id) => _flows[flow].GetPacketTypeById(id);

    public IPacket CreatePacket(PacketFlow flow, int id, MemoryStream stream)
        => _flows[flow].CreatePacket(id, stream);

    public T CreatePacket<T>(PacketFlow flow, int id, MemoryStream stream) where T : IPacket
        => _flows[flow].CreatePacket<T>(id, stream);
}