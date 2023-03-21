using DisasterPR.Cards;

namespace DisasterPR.Sessions;

public interface IChosenWordEntry
{
    public List<WordCard> Words { get; }
    public Guid PlayerId { get; } 
    public Guid Id { get; }
}