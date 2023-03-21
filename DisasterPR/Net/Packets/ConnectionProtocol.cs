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

    public static readonly ConnectionProtocol Login = new(PacketState.Login, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket<ClientboundDisconnectPacket>()
            .AddPacket<ClientboundAckPacket>()
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundLoginPacket>()
        )
    );
    
    public static readonly ConnectionProtocol Play = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket<ClientboundHeartbeatPacket>()
            .AddPacket<ClientboundChatPacket>()
            .AddPacket<ClientboundAddPlayerPacket>()
            .AddPacket<ClientboundRemovePlayerPacket>()
            .AddPacket<ClientboundJoinedRoomPacket>()
            .AddPacket<ClientboundSetCardPackPacket>()
            .AddPacket<ClientboundSetCandidateTopicsPacket>()
            .AddPacket<ClientboundSetTopicPacket>()
            .AddPacket<ClientboundSetWordsPacket>()
            .AddPacket<ClientboundRevealChosenWordEntryPacket>()
            .AddPacket<ClientboundSetFinalPacket>()
            .AddPacket<ClientboundSetWinnerPlayerPacket>()
            .AddPacket<ClientboundGameStateChangePacket>()
            .AddPacket<ClientboundGameCurrentPlayerChangePacket>()
            .AddPacket<ClientboundAddChosenWordEntryPacket>()
            .AddPacket<ClientboundUpdateSessionOptionsPacket>()
            .AddPacket<ClientboundUpdatePlayerScorePacket>()
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket<ServerboundHeartbeatPacket>()
            .AddPacket<ServerboundChatPacket>()
            .AddPacket<ServerboundHostRoomPacket>()
            .AddPacket<ServerboundJoinRoomPacket>()
            .AddPacket<ServerboundRequestRoomStartPacket>()
            .AddPacket<ServerboundChooseTopicPacket>()
            .AddPacket<ServerboundChooseWordPacket>()
            .AddPacket<ServerboundChooseFinalPacket>()
            .AddPacket<ServerboundRevealChosenWordEntryPacket>()
            .AddPacket<ServerboundUpdateSessionOptionsPacket>()
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

    private PacketSet GetPacketSetFromFlow(PacketFlow flow)
    {
        if (!_flows.ContainsKey(flow))
        {
            throw new KeyNotFoundException($"{flow} is not possible in state {PacketState}");
        }

        return _flows[flow];
    }
    
    public IPacket CreatePacket(PacketFlow flow, int id, MemoryStream stream)
        => GetPacketSetFromFlow(flow).CreatePacket(id, stream);

    public T CreatePacket<T>(PacketFlow flow, int id, MemoryStream stream) where T : IPacket
        => GetPacketSetFromFlow(flow).CreatePacket<T>(id, stream);
}