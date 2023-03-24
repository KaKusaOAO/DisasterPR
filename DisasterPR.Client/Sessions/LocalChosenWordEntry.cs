using DisasterPR.Cards;
using DisasterPR.Sessions;

namespace DisasterPR.Client.Sessions;

public class LocalChosenWordEntry : IChosenWordEntry
{
    public bool IsRevealed { get; set; }
    public LocalGameState GameState { get; }

    public AbstractClientPlayer? Player => GameState.Session.Players.Find(c => c.Id == PlayerId);
    public Guid? PlayerId { get; set; }
    
    public Guid Id { get; }

    private List<WordCard> _words;

    public List<WordCard> Words => !_words.Any()
        ? Enumerable.Repeat(EmptyWordCard.Instance, GameState.CurrentTopic.AnswerCount).ToList<WordCard>()
        : _words;

    public LocalChosenWordEntry(Guid id, LocalGameState state, AbstractClientPlayer? player, List<WordCard> words)
    {
        Id = id;
        GameState = state;
        PlayerId = player?.Id;
        _words = words;
    }
}