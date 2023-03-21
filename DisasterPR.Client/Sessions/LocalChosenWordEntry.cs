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

    public List<WordCard> Words { get; }

    public LocalChosenWordEntry(Guid id, LocalGameState state, AbstractClientPlayer player, List<WordCard> words)
    {
        Id = id;
        GameState = state;
        Player = player;
        Words = words;
    }
}