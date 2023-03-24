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
            .AddPacket(s => new ServerboundHelloPacket(s))
        )
    );

    public static readonly ConnectionProtocol Login = new(PacketState.Login, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket(s => new ClientboundDisconnectPacket(s))
            .AddPacket(s => new ClientboundAckPacket(s))
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket(s => new ServerboundLoginPacket(s))
        )
    );
    
    public static readonly ConnectionProtocol Play = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket(s => new ClientboundRoomDisconnectedPacket(s))
            .AddPacket(s => new ClientboundHeartbeatPacket(s))
            .AddPacket(s => new ClientboundChatPacket(s))
            .AddPacket(s => new ClientboundAddPlayerPacket(s))
            .AddPacket(s => new ClientboundRemovePlayerPacket(s))
            .AddPacket(s => new ClientboundJoinedRoomPacket(s))
            .AddPacket(s => new ClientboundSetCardPackPacket(s))
            .AddPacket(s => new ClientboundSetCandidateTopicsPacket(s))
            .AddPacket(s => new ClientboundSetTopicPacket(s))
            .AddPacket(s => new ClientboundSetWordsPacket(s))
            .AddPacket(s => new ClientboundRevealChosenWordEntryPacket(s))
            .AddPacket(s => new ClientboundSetFinalPacket(s))
            .AddPacket(s => new ClientboundSetWinnerPlayerPacket(s))
            .AddPacket(s => new ClientboundGameStateChangePacket(s))
            .AddPacket(s => new ClientboundGameCurrentPlayerChangePacket(s))
            .AddPacket(s => new ClientboundAddChosenWordEntryPacket(s))
            .AddPacket(s => new ClientboundUpdateSessionOptionsPacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerScorePacket(s))
            .AddPacket(s => new ClientboundUpdateTimerPacket(s))
            .AddPacket(s => new ClientboundUpdateRoundCyclePacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerStatePacket(s))
            .AddPacket(s => new ClientboundReplacePlayerPacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerGuidPacket(s))
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket(s => new ServerboundHeartbeatPacket(s))
            .AddPacket(s => new ServerboundChatPacket(s))
            .AddPacket(s => new ServerboundHostRoomPacket(s))
            .AddPacket(s => new ServerboundJoinRoomPacket(s))
            .AddPacket(s => new ServerboundLeaveRoomPacket(s))
            .AddPacket(s => new ServerboundRequestRoomStartPacket(s))
            .AddPacket(s => new ServerboundChooseTopicPacket(s))
            .AddPacket(s => new ServerboundChooseWordPacket(s))
            .AddPacket(s => new ServerboundChooseFinalPacket(s))
            .AddPacket(s => new ServerboundRevealChosenWordEntryPacket(s))
            .AddPacket(s => new ServerboundUpdateSessionOptionsPacket(s))
            .AddPacket(s => new ServerboundRequestKickPlayerPacket(s))
            .AddPacket(s => new ServerboundUpdatePlayerStatePacket(s))
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