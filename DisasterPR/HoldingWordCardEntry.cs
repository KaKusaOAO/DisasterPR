using DisasterPR.Cards;

namespace DisasterPR;

public class HoldingWordCardEntry
{
    public WordCard Card { get; }

    public HoldingWordCardEntry(WordCard card)
    {
        Card = card;
    }
}