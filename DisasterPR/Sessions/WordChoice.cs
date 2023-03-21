using DisasterPR.Cards;
using KaLib.Utils.Extensions;

namespace DisasterPR.Sessions;

public class WordChoice
{
    public List<WordCard> Words { get; set; } = new();

    public override string ToString() => "[" + Words.Select(w => w.Label).JoinStrings(", ") + "]";
}