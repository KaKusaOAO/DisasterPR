using DisasterPR.Cards;

namespace DisasterPR.Sessions;

public interface IGameState
{
    public ISession Session { get; }
    public StateOfGame CurrentState { get; }
    public int CurrentPlayerIndex { get; set; }
    public IPlayer CurrentPlayer { get; }
    public IPlayer? WinnerPlayer { get; }
    public TopicCard CurrentTopic { get; set; }
    public List<IChosenWordEntry> CurrentChosenWords { get; }
    public (TopicCard Left, TopicCard Right)? CandidateTopics { get; }
    public int RoundCycle { get; set; }
}

public enum StateOfGame
{
    Waiting,
    Started,
    ChoosingTopic,
    ChoosingWord,
    ChoosingFinal,
    PrepareNextRound,
    WinResult
}