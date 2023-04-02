using DisasterPR.Client.Sessions;
using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using Mochi.Utils;
using Mochi.Utils.Extensions;

namespace DisasterPR.Client.Net.Packets.Play;

public class ClientPlayPacketHandler : IClientPlayPacketHandler
{
    public PlayerToServerConnection Connection { get; }
    public LocalPlayer Player => Connection.Player;

    public ClientPlayPacketHandler(PlayerToServerConnection connection)
    {
        Connection = connection;
    }
    
    public void HandleAddPlayer(ClientboundAddPlayerPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;

            var player = new RemotePlayer(packet.PlayerId, packet.PlayerName);
            await session.PlayerJoinAsync(player);
        }).Wait();
    }

    public async void HandleRemovePlayer(ClientboundRemovePlayerPacket packet)
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

    public async void HandleGameStateChange(ClientboundGameStateChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        await session.LocalGameState.TransitionToStateAsync(packet.State);
    }

    public async void HandleGameCurrentPlayerChange(ClientboundGameCurrentPlayerChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var state = session.LocalGameState;
        state.CurrentPlayerIndex = packet.Index;
        state.OnCurrentPlayerUpdated();
    }

    public async void HandleChat(ClientboundChatPacket packet)
    {
        Game.Instance.InternalOnPlayerChat(new PlayerChatEventArgs
        {
            PlayerName = packet.Player,
            Content = packet.Content
        });  
    }

    public void HandleRoomDisconnected(ClientboundRoomDisconnectedPacket packet)
    {
        Player.Session?.Invalidate();
        Player.Session = null;
    }

    public void HandleHeartbeat(ClientboundHeartbeatPacket packet) {}
    
    public void HandleJoinedRoom(ClientboundJoinedRoomPacket packet)
    {
        var session = new LocalSession();
        session.Players.AddRange(packet.Players.Select(p => new RemotePlayer(p)
        {
            Session = session
        }));
        
        session.Players.Add(Player);
        session.RoomId = packet.RoomId;
        
        Player.Session = session;
    }

    public async void HandleSetCardPack(ClientboundSetCardPackPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.CardPack = packet.CardPack;
    }

    public async void HandleSetCandidateTopics(ClientboundSetCandidateTopicsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var pack = session.CardPack;
        var left = pack.Topics[packet.Left];
        var right = pack.Topics[packet.Right];
        session.LocalGameState.CandidateTopics = (left, right);
    }

    public async void HandleSetTopic(ClientboundSetTopicPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var pack = session.CardPack;
        session.LocalGameState.CurrentTopic = pack.Topics[packet.Index];
    }

    public async void HandleSetWords(ClientboundSetWordsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var pack = session.CardPack;
        var words = packet.Entries
            .Select(i => new HoldingWordCardEntry(pack.Words[i.Index], i.IsLocked));
        Player.HoldingCards.Clear();
        Player.HoldingCards.AddRange(words);
    }

    public async void HandleAddChosenWordEntry(ClientboundAddChosenWordEntryPacket packet)
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

    public async void HandleUpdateSessionOptions(ClientboundUpdateSessionOptionsPacket packet)
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

    public async void HandleRevealChosenWordEntry(ClientboundRevealChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords.Find(w => w.Id == packet.Guid);
        chosen.IsRevealed = true;
        Logger.Info($"Revealed chosen word: {chosen.Words.Select(w => w.Label).JoinStrings(", ")}");
    }

    public async void HandleSetFinal(ClientboundSetFinalPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords[packet.Index];
        session.LocalGameState.FinalChosenWord = chosen;
        Logger.Info($"Chosen final word: {chosen.Words.Select(w => w.Label).JoinStrings(", ")}");
    }

    public async void HandleUpdatePlayerScore(ClientboundUpdatePlayerScorePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var player = session.Players.Find(p => p.Id == packet.PlayerId);
        player.Score = packet.Score;
    }

    public async void HandleSetWinnerPlayer(ClientboundSetWinnerPlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.LocalGameState.WinnerPlayer = session.Players.Find(p => p.Id == packet.PlayerId);
    }

    public async void HandleUpdateTimer(ClientboundUpdateTimerPacket packet) 
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.LocalGameState.CurrentTimer = packet.RemainTime;
    }

    public async void HandleUpdateRoundCycle(ClientboundUpdateRoundCyclePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        session.LocalGameState.RoundCycle = packet.Count;
    }

    public async void HandleUpdatePlayerState(ClientboundUpdatePlayerStatePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        var player = session.Players.First(c => packet.Id == c.Id);
        player.State = packet.State;
    }

    public void HandleReplacePlayer(ClientboundReplacePlayerPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;

            var player = new RemotePlayer(packet.PlayerId, packet.PlayerName);
            await session.PlayerReplaceAsync(packet.Index, player);
        }).Wait();
    }

    public void HandleUpdatePlayerGuid(ClientboundUpdatePlayerGuidPacket packet)
    {
        Player.Id = packet.Guid;
    }

    public void HandleSystemChat(ClientboundSystemChatPacket packet)
    {
        // implement this later?
    }
    
    public void HandleUpdateLockedWord(ClientboundUpdateLockedWordPacket packet)
    {
        Player.HoldingCards[packet.Index].IsLocked = packet.IsLocked;
    }
}