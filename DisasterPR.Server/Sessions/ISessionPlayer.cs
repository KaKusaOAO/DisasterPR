using DisasterPR.Cards;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.Nbt;
using ISession = DisasterPR.Sessions.ISession;
using LogLevel = Mochi.Utils.LogLevel;

namespace DisasterPR.Server.Sessions;

public interface ISessionPlayer : IPlayer
{
    public event DisconnectedEventDelegate Disconnected;
    
    public bool IsConnected { get; }
    
    public new ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    public ShuffledPool<WordCard> CardPool { get; set; }
    
    public bool IsManuallyShuffled { get; set; }

    public Task SetCardPackAsync(CardPack pack);
    public Task UpdateSessionOptions(ServerSession session);

    public Task SendJoinRoomSequenceAsync(ServerSession session, int? selfIndex = null);
    
    /// <summary>
    /// Invoked when the session this player in has a newly joined player.
    /// </summary>
    /// <param name="player">The newly joined player.</param>
    /// <returns>The running task to complete.</returns>
    public Task OnNewPlayerJoinedSessionAsync(ISessionPlayer player);
    public Task OnPlayerLeftSessionAsync(ISessionPlayer player);
    public Task OnOtherPlayerUpdateStateAsync(ISessionPlayer player);
    public Task OnReplaceSessionPlayerAsync(int index, ISessionPlayer player);
    public Task KickFromSessionAsync(RoomDisconnectReason reason);
    public Task KickFromSessionAsync(string reason);
    public Task UpdateSessionGameStateAsync(StateOfGame state);
    public Task UpdateCurrentPlayerIndexAsync(int index);
    public Task UpdatePlayerScoreAsync(ISessionPlayer player, int score);
    public Task UpdateWinnerPlayerAsync(Guid id);
    public Task AddChosenWordEntryAsync(Guid id, Guid? playerId, List<int> indices);
    public Task OnSessionChat(string name, string content);
    public Task UpdatePlayerStateAsync(ISessionPlayer player);
    public Task UpdateCandidateTopicsAsync(int left, int right);
    public Task UpdateTimerAsync(int timer);
    public Task UpdateCurrentTopicAsync(int id);
    public Task UpdateHoldingWordsAsync(List<HoldingWordCardEntry> entries);
    public Task RevealChosenWordEntryAsync(Guid id);
    public Task UpdateFinalWordCardAsync(int index);
    public Task UpdateRoundCycleAsync(int cycle);
    public Task SendToastAsync(string message, LogLevel level = LogLevel.Info);

    public void ShuffleHoldingCards()
    {
        DefaultShuffleHoldingCards(this);
    }

    protected static void DefaultShuffleHoldingCards(ISessionPlayer p)
    {
        var words = new List<HoldingWordCardEntry>();
        words.AddRange(p.HoldingCards.Where(w => w.IsLocked));
            
        var shuffled = p.CardPool.Items.Shuffled().ToList();
        var newWords = new List<HoldingWordCardEntry>();
        newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Noun)
            .Take(5)
            .Select(w => new HoldingWordCardEntry(w, false)));
        newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Verb)
            .Take(4)
            .Select(w => new HoldingWordCardEntry(w, false)));
        newWords.AddRange(shuffled.Where(w => w.PartOfSpeech == PartOfSpeech.Adjective)
            .Take(2)
            .Select(w => new HoldingWordCardEntry(w, false)));
        words.AddRange(newWords.Shuffled());
            
        p.HoldingCards.Clear();
        p.HoldingCards.AddRange(words.Take(11));
    }
}

public static class SessionPlayerExtension
{
    public static NbtCompound CreateSnapshot(this ISessionPlayer p)
    {
        var pt = new NbtCompound
        {
            { "Name", p.Name },
            { "Id", p.Id.ToString() },
            { "Score", p.Score },
            { "State", Enum.GetName(p.State) }
        };

        var hList = new NbtList();
        foreach (var h in p.HoldingCards)
        {
            var ht = new NbtCompound();
            ht["Locked"] = h.IsLocked;

            var ct = new NbtCompound();
            ct["Label"] = h.Card.Label;
            ct["Pos"] = Enum.GetName(h.Card.PartOfSpeech);
            ht["Card"] = ct;
            
            hList.Add(ht);
        }
        
        pt["HoldingCards"] = hList;
        return pt;
    }
}