using DisasterPR.Cards;
using DisasterPR.Sessions;

namespace DisasterPR.Server.Sessions;

public class ServerChosenWordEntry : IChosenWordEntry
{
    public ServerGameState GameState { get; set; }
    
    public Guid? PlayerId { get; set; }
    public ISessionPlayer? Player => GameState.Session.Players.Find(c => c.Id == PlayerId);

    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsRevealed { get; set; }

    private List<WordCard> _words;

    public List<WordCard> Words => !_words.Any()
        ? Enumerable.Repeat(EmptyWordCard.Instance, GameState.CurrentTopic.AnswerCount).ToList<WordCard>()
        : _words;

    public ServerChosenWordEntry(ServerGameState state, ISessionPlayer? player, List<WordCard> words)
    {
        GameState = state;
        PlayerId = player?.Id;
        _words = words;
    }
}