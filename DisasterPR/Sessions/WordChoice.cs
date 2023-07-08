using DisasterPR.Cards;
using Mochi.Utils.Extensions;

namespace DisasterPR.Sessions;

public class WordChoice
{
    public List<WordCard> Words { get; set; } = new();

    public override string ToString() => "[" + string.Join(", ", Words.Select(w => w.Label)) + "]";
}