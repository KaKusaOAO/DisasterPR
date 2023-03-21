using DisasterPR.Cards;
using DisasterPR.Sessions;

namespace DisasterPR.Client.Sessions;

public class LocalChosenWordEntry : IChosenWordEntry
{
    public bool IsRevealed { get; set; }
    public LocalGameState GameState { get; }
    
    public AbstractClientPlayer Player { get; }
    Guid IChosenWordEntry.PlayerId => Player.Id;
    
    public Guid Id { get; }

    private List<WordCard> _words;

    public List<WordCard> Words => !_words.Any()
        ? Enumerable.Repeat(new EmptyWordCard(), GameState.CurrentTopic.AnswerCount).ToList<WordCard>()
        : _words;

    public LocalChosenWordEntry(Guid id, LocalGameState state, AbstractClientPlayer player, List<WordCard> words)
    {
        Id = id;
        GameState = state;
        Player = player;
        _words = words;
    }
}