using DisasterPR.Cards;
using DisasterPR.Sessions;

namespace DisasterPR.Server.Sessions;

public class ServerChosenWordEntry : IChosenWordEntry
{
    public ServerGameState GameState { get; set; }
    
    public ServerPlayer? Player { get; }
    Guid? IChosenWordEntry.PlayerId => Player?.Id;

    public Guid Id { get; set; } = Guid.NewGuid();

    private List<WordCard> _words;

    public List<WordCard> Words => !_words.Any()
        ? Enumerable.Repeat(EmptyWordCard.Instance, GameState.CurrentTopic.AnswerCount).ToList<WordCard>()
        : _words;

    public ServerChosenWordEntry(ServerGameState state, ServerPlayer? player, List<WordCard> words)
    {
        GameState = state;
        Player = player;
        _words = words;
    }
}