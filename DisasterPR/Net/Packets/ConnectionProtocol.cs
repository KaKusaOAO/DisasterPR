using System.Reflection;
using System.Text.Json.Nodes;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using Mochi.IO;

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
            .AddPacket(s => new ClientboundAckLoginPacket(s))
            .AddPacket(s => new ClientboundSystemChatPacket(s))
        )
        .AddFlow(PacketFlow.Serverbound, new PacketSet()
            .AddPacket(s => new ServerboundLoginPacket(s))
        )
    );
    
    public static readonly ConnectionProtocol Play = new(PacketState.Play, Protocol()
        .AddFlow(PacketFlow.Clientbound, new PacketSet()
            .AddPacket(s => new ClientboundRoomDisconnectedPacket(s), s => new ClientboundRoomDisconnectedPacket(s))
            .AddPacket(s => new ClientboundHeartbeatPacket(s))
            .AddPacket(s => new ClientboundChatPacket(s), s => new ClientboundChatPacket(s))
            .AddPacket(s => new ClientboundAddPlayerPacket(s), s => new ClientboundAddPlayerPacket(s))
            .AddPacket(s => new ClientboundRemovePlayerPacket(s), s => new ClientboundRemovePlayerPacket(s))
            .AddPacket(s => new ClientboundJoinedRoomPacket(s), s => new ClientboundJoinedRoomPacket(s))
            .AddPacket(s => new ClientboundSetCardPackPacket(s), s => new ClientboundSetCardPackPacket(s))
            .AddPacket(s => new ClientboundSetCandidateTopicsPacket(s), s => new ClientboundSetCandidateTopicsPacket(s))
            .AddPacket(s => new ClientboundSetTopicPacket(s), s => new ClientboundSetTopicPacket(s))
            .AddPacket(s => new ClientboundSetWordsPacket(s), s => new ClientboundSetWordsPacket(s))
            .AddPacket(s => new ClientboundRevealChosenWordEntryPacket(s))
            .AddPacket(s => new ClientboundSetFinalPacket(s), s => new ClientboundSetFinalPacket(s))
            .AddPacket(s => new ClientboundSetWinnerPlayerPacket(s), s => new ClientboundSetWinnerPlayerPacket(s))
            .AddPacket(s => new ClientboundGameStateChangePacket(s), s => new ClientboundGameStateChangePacket(s))
            .AddPacket(s => new ClientboundGameCurrentPlayerChangePacket(s))
            .AddPacket(s => new ClientboundAddChosenWordEntryPacket(s), s => new ClientboundAddChosenWordEntryPacket(s))
            .AddPacket(s => new ClientboundUpdateSessionOptionsPacket(s), s => new ClientboundUpdateSessionOptionsPacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerScorePacket(s), s => new ClientboundUpdatePlayerScorePacket(s))
            .AddPacket(s => new ClientboundUpdateTimerPacket(s), s => new ClientboundUpdateTimerPacket(s))
            .AddPacket(s => new ClientboundUpdateRoundCyclePacket(s), s => new ClientboundUpdateRoundCyclePacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerStatePacket(s), s => new ClientboundUpdatePlayerStatePacket(s))
            .AddPacket(s => new ClientboundReplacePlayerPacket(s), s => new ClientboundReplacePlayerPacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerGuidPacket(s), s => new ClientboundUpdatePlayerGuidPacket(s))
            .AddPacket(s => new ClientboundUpdatePlayerDataPacket(s), s => new ClientboundUpdatePlayerDataPacket(s))
            .AddPacket(s => new ClientboundSystemChatPacket(s), s => new ClientboundSystemChatPacket(s))
            .AddPacket(s => new ClientboundUpdateLockedWordPacket(s), s => new ClientboundUpdateLockedWordPacket(s))
            .AddPacket(s => new ClientboundRandomNameResponsePacket(s), s => new ClientboundRandomNameResponsePacket(s))
            .AddPacket(_ => new ClientboundDismissNameChangeModalPacket())
            .AddPacket(s => new ClientboundUpdateSessionSeedPacket(s), s => new ClientboundUpdateSessionSeedPacket(s))
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
            .AddPacket(s => new ServerboundRequestKickPlayerPacket(s), s => new ServerboundRequestKickPlayerPacket(s))
            .AddPacket(s => new ServerboundUpdatePlayerStatePacket(s))
            .AddPacket(s => new ServerboundUpdateLockedWordPacket(s))
            .AddPacket(s => new ServerboundRequestShuffleWordsPacket(s))
            .AddPacket(s => new ServerboundRequestRandomNamePacket(s), s => new ServerboundRequestRandomNamePacket(s))
            .AddPacket(s => new ServerboundRequestUpdateNamePacket(s), s => new ServerboundRequestUpdateNamePacket(s))
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
    
    public IPacket CreatePacket(PacketFlow flow, int id, BufferReader stream)
        => GetPacketSetFromFlow(flow).CreatePacket(id, stream);
    
    public IPacket CreatePacket(PacketFlow flow, int id, JsonObject stream)
        => GetPacketSetFromFlow(flow).CreatePacket(id, stream);

    public T CreatePacket<T>(PacketFlow flow, int id, BufferReader stream) where T : IPacket
        => GetPacketSetFromFlow(flow).CreatePacket<T>(id, stream);
    
    public T CreatePacket<T>(PacketFlow flow, int id, JsonObject stream) where T : IPacket
        => GetPacketSetFromFlow(flow).CreatePacket<T>(id, stream);
}