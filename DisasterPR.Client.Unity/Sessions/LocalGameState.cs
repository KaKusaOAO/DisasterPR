using DisasterPR.Cards;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using Mochi.Utils;

namespace DisasterPR.Client.Unity.Sessions;

public class LocalGameState : IGameState
{
    public LocalSession Session { get; }
    ISession IGameState.Session => Session;

    public StateOfGame CurrentState { get; private set; } = StateOfGame.Waiting;
    public int CurrentPlayerIndex { get; set; }
    
    public AbstractClientPlayer CurrentPlayer => Session.Players[CurrentPlayerIndex];
    IPlayer IGameState.CurrentPlayer => CurrentPlayer;
    
    public AbstractClientPlayer? WinnerPlayer { get; set; }
    IPlayer IGameState.WinnerPlayer => WinnerPlayer;
    
    public TopicCard CurrentTopic { get; set; }
    public int CurrentTimer { get; set; }
    
    public List<LocalChosenWordEntry> CurrentChosenWords { get; } = new();
    public LocalChosenWordEntry? FinalChosenWord { get; set; }

    public event Action StateTransitioned;
    public event Action CurrentPlayerUpdated;

    List<IChosenWordEntry> IGameState.CurrentChosenWords =>
        CurrentChosenWords.Select(n => (IChosenWordEntry) n).ToList();
    
    public (TopicCard Left, TopicCard Right)? CandidateTopics { get; set;  }
    public int RoundCycle { get; set; }

    public LocalGameState(LocalSession session)
    {
        Session = session;
    }
    
    public Task StartAsync()
    {
        CurrentState = StateOfGame.Started;
        return Task.CompletedTask;
    }

    private void CheckSession()
    {
        if (!Session.IsValid)
        {
            throw new InvalidOperationException("Session is invalid.");
        }
    }
    
    public void ChooseTopic(HorizontalSide side)
    {
        CheckSession();
        
        var player = Game.Instance.Player;
        if (CurrentState != StateOfGame.ChoosingTopic) return;
        if (CurrentPlayer != player) return;
        player.Connection.SendPacket(new ServerboundChooseTopicPacket(side));
    }

    public void ChooseWord(HashSet<int> cards)
    {
        CheckSession();

        var player = Game.Instance.Player!;
        if (CurrentState != StateOfGame.ChoosingWord) return;
        if (cards.Count != CurrentTopic.AnswerCount)
        {
            Logger.Warn("Chosen word count doesn't match the topic word count");
            return;
        }
        
        player.Connection.SendPacket(new ServerboundChooseWordPacket(cards));
    }

    public void TransitionToState(StateOfGame state)
    {
        CheckSession();
        CurrentState = state;

        if (state == StateOfGame.ChoosingTopic)
        {
            CurrentChosenWords.Clear();
            FinalChosenWord = null;
        }
        
        if (state == StateOfGame.ChoosingWord)
        {
            CandidateTopics = null;
        }

        if (state == StateOfGame.Started)
        {
            WinnerPlayer = null;
        }
        
        StateTransitioned?.Invoke();
    }
    
    public void RevealChosenWordEntry(Guid guid)
    {
        CheckSession();

        var player = Game.Instance.Player!;
        if (CurrentState != StateOfGame.ChoosingFinal)
        {
            throw new InvalidOperationException("Attempted to reveal cards when it's not the time to do it");
        }

        player.Connection.SendPacket(new ServerboundRevealChosenWordEntryPacket(guid));
    }
    
    public void ChooseFinal(int index)
    {
        CheckSession();

        var player = Game.Instance.Player!;
        if (CurrentState != StateOfGame.ChoosingFinal) return;
        player.Connection.SendPacket(new ServerboundChooseFinalPacket(index));
    }

    internal void OnCurrentPlayerUpdated() => CurrentPlayerUpdated?.Invoke();
}