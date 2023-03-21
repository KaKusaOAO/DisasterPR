using DisasterPR.Cards;
using DisasterPR.Sessions;

namespace DisasterPR.Server.Sessions;

public class ServerChosenWordEntry : IChosenWordEntry
{
    public ServerGameState GameState { get; set; }
    
    public ServerPlayer Player { get; }
    Guid IChosenWordEntry.PlayerId => Player.Id;

    public Guid Id { get; set; } = Guid.NewGuid();

    public List<WordCard> Words { get; }

    public ServerChosenWordEntry(ServerGameState state, ServerPlayer player, List<WordCard> words)
    {
        GameState = state;
        Player = player;
        Words = words;
    }
}