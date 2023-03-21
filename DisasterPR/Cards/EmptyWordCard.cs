using KaLib.Utils;

namespace DisasterPR.Cards;

public class EmptyWordCard : WordCard
{
    public static readonly EmptyWordCard Instance = new();
    
    public override string Label
    {
        get => "(空空如也)"; 
        set => Logger.Warn("Setting label of empty word card is not supported");
    }

    private EmptyWordCard()
    {
        
    }
}