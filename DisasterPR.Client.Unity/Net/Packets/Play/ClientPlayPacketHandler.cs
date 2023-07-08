using DisasterPR.Client.Unity.Sessions;
using DisasterPR.Events;
using DisasterPR.Net.Packets.Play;
using Mochi.Utils;
using Mochi.Utils.Extensions;

namespace DisasterPR.Client.Unity.Net.Packets.Play;

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
        var session = Player.Session;
        if (session == null) return;

        var player = new RemotePlayer(packet.PlayerId, packet.PlayerName);
        session.PlayerJoin(player);
    }

    public void HandleRemovePlayer(ClientboundRemovePlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var removal = session.Players.Where(p => p.Id == packet.PlayerId).ToList();
        foreach (var player in removal)
        {
            session.PlayerLeave(player);
        }
    }

    public void HandleGameStateChange(ClientboundGameStateChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        session.LocalGameState.TransitionToState(packet.State);
    }

    public void HandleGameCurrentPlayerChange(ClientboundGameCurrentPlayerChangePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var state = session.LocalGameState;
        state.CurrentPlayerIndex = packet.Index;
        state.OnCurrentPlayerUpdated();
    }

    public void HandleChat(ClientboundChatPacket packet)
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

        if (packet.SelfIndex.HasValue)
        {
            session.Players.Insert(packet.SelfIndex.Value, Player);
        }
        else
        {
            session.Players.Add(Player);
        }

        session.RoomId = packet.RoomId;
        
        Player.Session = session;
    }

    public void HandleSetCardPack(ClientboundSetCardPackPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        session.CardPack = packet.CardPack;
    }

    public void HandleSetCandidateTopics(ClientboundSetCandidateTopicsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var pack = session.CardPack!;
        var left = pack.Topics[packet.Left];
        var right = pack.Topics[packet.Right];
        session.LocalGameState.CandidateTopics = (left, right);
    }

    public void HandleSetTopic(ClientboundSetTopicPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        
        var pack = session.CardPack!;
        session.LocalGameState.CurrentTopic = pack.Topics[packet.Index];
    }

    public void HandleSetWords(ClientboundSetWordsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        
        var pack = session.CardPack!;
        var words = packet.Entries
            .Select(i => new HoldingWordCardEntry(pack.Words[i.Index], i.IsLocked));
        Player.HoldingCards.Clear();
        Player.HoldingCards.AddRange(words);
    }

    public void HandleAddChosenWordEntry(ClientboundAddChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        
        var pack = session.CardPack!;
        var words = packet.Words.Select(i => pack.Words[i]).ToList();
        var player = session.Players.Find(p => p.Id == packet.PlayerId);
        var state = session.LocalGameState;
        state.CurrentChosenWords.Add(new LocalChosenWordEntry(packet.Id, state, player, words));
    }

    public void HandleUpdateSessionOptions(ClientboundUpdateSessionOptionsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var options = session.Options;
        options.WinScore = packet.WinScore;
        options.CountdownTimeSet = packet.CountdownTimeSet;
        options.EnabledCategories = packet.EnabledCategories
            .Select(g => session.CardPack!.Categories.First(c => c.Guid == g)).ToList();
    }

    public void HandleRevealChosenWordEntry(ClientboundRevealChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords.Find(w => w.Id == packet.Guid);
        chosen.IsRevealed = true;
        Logger.Info($"Revealed chosen word: {string.Join(", ", chosen.Words.Select(w => w.Label))}");
    }

    public void HandleSetFinal(ClientboundSetFinalPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        
        var state = session.LocalGameState;
        var chosen = state.CurrentChosenWords[packet.Index];
        session.LocalGameState.FinalChosenWord = chosen;
        Logger.Info($"Chosen final word: {string.Join(", ", chosen.Words.Select(w => w.Label))}");
    }

    public void HandleUpdatePlayerScore(ClientboundUpdatePlayerScorePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var player = session.Players.Find(p => p.Id == packet.PlayerId);
        player.Score = packet.Score;
    }

    public void HandleSetWinnerPlayer(ClientboundSetWinnerPlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        session.LocalGameState.WinnerPlayer = session.Players.Find(p => p.Id == packet.PlayerId);
    }

    public void HandleUpdateTimer(ClientboundUpdateTimerPacket packet) 
    {
        var session = Player.Session;
        if (session == null) return;

        session.LocalGameState.CurrentTimer = packet.RemainTime;
    }

    public void HandleUpdateRoundCycle(ClientboundUpdateRoundCyclePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        session.LocalGameState.RoundCycle = packet.Count;
    }

    public void HandleUpdatePlayerState(ClientboundUpdatePlayerStatePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var player = session.Players.First(c => packet.Id == c.Id);
        player.State = packet.State;
    }

    public void HandleReplacePlayer(ClientboundReplacePlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var player = new RemotePlayer(packet.PlayerId, packet.PlayerName);
        session.PlayerReplace(packet.Index, player);
    }

    public void HandleUpdatePlayerGuid(ClientboundUpdatePlayerGuidPacket packet)
    {
        Player.Id = packet.Guid;
    }

    public void HandleSystemChat(ClientboundSystemChatPacket packet)
    {
        
    }

    public void HandleUpdateLockedWord(ClientboundUpdateLockedWordPacket packet)
    {
        Player.HoldingCards[packet.Index].IsLocked = packet.IsLocked;
    }

    public void HandleUpdatePlayerName(ClientboundUpdatePlayerNamePacket packet)
    {
        Player.Name = packet.Name;
    }
}