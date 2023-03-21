using DisasterPR.Client.Sessions;
using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using KaLib.Utils;
using KaLib.Utils.Extensions;

namespace DisasterPR.Client.Net.Packets.Play;

public class ClientPlayPacketHandler : IClientPlayPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientPlayPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }
    
    public async Task HandleAddPlayerAsync(ClientboundAddPlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var player = new RemotePlayer(packet.PlayerId, packet.PlayerName);
        await session.PlayerJoinAsync(player);
    }

    public async Task HandleRemovePlayerAsync(ClientboundRemovePlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var removal = session.Players.Where(p => p.Id == packet.PlayerId).ToList();
        foreach (var player in removal)
        {
            await session.PlayerLeaveAsync(player);
        }
    }

    public async Task HandleGameStateChangeAsync(ClientboundGameStateChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        await session.LocalGameState.TransitionToStateAsync(packet.State);
    }

    public async Task HandleGameCurrentPlayerChangeAsync(ClientboundGameCurrentPlayerChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var state = session.LocalGameState;
        state.CurrentPlayerIndex = packet.Index;
        state.OnCurrentPlayerUpdated();
    }

    public async Task HandleChatAsync(ClientboundChatPacket packet)
    {
        Game.Instance.InternalOnPlayerChat(new PlayerChatEventArgs
        {
            PlayerName = packet.Player,
            Content = packet.Content
        });  
    }

    public Task HandleRoomDisconnectedAsync(ClientboundRoomDisconnectedPacket packet)
    {
        Player.Session?.Invalidate();
        Player.Session = null;
        return Task.CompletedTask;
    }

    public Task HandleHeartbeatAsync(ClientboundHeartbeatPacket packet) => Task.CompletedTask;
    
    public Task HandleJoinedRoomAsync(ClientboundJoinedRoomPacket packet)
    {
        var session = new LocalSession();
        session.Players.AddRange(packet.Players.Select(p => new RemotePlayer(p)
        {
            Session = session
        }));
        
        session.Players.Add(Player);
        session.RoomId = packet.RoomId;
        
        Player.Session = session;
        return Task.CompletedTask;
    }

    public async Task HandleSetCardPackAsync(ClientboundSetCardPackPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.CardPack = packet.CardPack;
    }

    public async Task HandleSetCandidateTopicsPacket(ClientboundSetCandidateTopicsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var pack = session.CardPack;
        var left = pack.Topics[packet.Left];
        var right = pack.Topics[packet.Right];
        session.LocalGameState.CandidateTopics = (left, right);
    }

    public async Task HandleSetTopicPacket(ClientboundSetTopicPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var pack = session.CardPack;
        session.LocalGameState.CurrentTopic = pack.Topics[packet.Index];
    }

    public async Task HandleSetWordsPacket(ClientboundSetWordsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var pack = session.CardPack;
        var words = packet.Words.Select(i => pack.Words[i]);
        Player.HoldingCards.Clear();
        Player.HoldingCards.AddRange(words);
    }

    public async Task HandleAddChosenWordEntryAsync(ClientboundAddChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var pack = session.CardPack;
        var words = packet.Words.Select(i => pack.Words[i]).ToList();
        var player = session.Players.Find(p => p.Id == packet.PlayerId);
        var state = session.LocalGameState;
        state.CurrentChosenWords.Add(new LocalChosenWordEntry(packet.Id, state, player, words));
    }

    public async Task HandleUpdateSessionOptionsPacket(ClientboundUpdateSessionOptionsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var options = session.Options;
        options.WinScore = packet.WinScore;
        options.CountdownTimeSet = packet.CountdownTimeSet;
        options.EnabledCategories = packet.EnabledCategories
            .Select(g => session.CardPack.Categories.First(c => c.Guid == g)).ToList();
    }

    public async Task HandleRevealChosenWordEntryAsync(ClientboundRevealChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords.Find(w => w.Id == packet.Guid);
        chosen.IsRevealed = true;
        Logger.Info($"Revealed chosen word: {chosen.Words.Select(w => w.Label).JoinStrings(", ")}");
    }

    public async Task HandleSetFinalPacket(ClientboundSetFinalPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords[packet.Index];
        session.LocalGameState.FinalChosenWord = chosen;
        Logger.Info($"Chosen final word: {chosen.Words.Select(w => w.Label).JoinStrings(", ")}");
    }

    public async Task HandleUpdatePlayerScoreAsync(ClientboundUpdatePlayerScorePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var player = session.Players.Find(p => p.Id == packet.PlayerId);
        player.Score = packet.Score;
    }

    public async Task HandleSetWinnerPlayerPacket(ClientboundSetWinnerPlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.LocalGameState.WinnerPlayer = session.Players.Find(p => p.Id == packet.PlayerId);
    }
}