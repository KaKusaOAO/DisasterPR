using DisasterPR.Cards;

namespace DisasterPR;

public class HoldingWordCardEntry
{
    /// <summary>
    /// 這張卡牌是否被鎖定。
    /// 鎖定的卡牌會在每一輪發新卡牌的時候被保留，而每一張被鎖定的卡牌都無法被提交。
    /// </summary>
    public bool IsLocked { get; set; }
    
    public WordCard Card { get; }

    public HoldingWordCardEntry(WordCard card, bool locked)
    {
        IsLocked = locked;
        Card = card;
    }
}