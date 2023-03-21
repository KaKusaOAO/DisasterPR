using System.Reflection.Emit;
using DisasterPR.Extensions;

namespace DisasterPR.Cards;

public class TopicCard
{
    public List<CardCategory> Categories { get; set; }
    public List<string> Texts { get; set; } = new();

    public int AnswerCount => Texts.Count - 1;
    
    public static TopicCard Deserialize(CardPack pack, Stream stream)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");

        return new TopicCard
        {
            Categories = stream.ReadList(s => CardCategory.DeserializeNoLabel(s, pack)),
            Texts = stream.ReadList(s => s.ReadUtf8String())
        };
    }

    public void Serialize(Stream stream)
    {
        stream.WriteList(Categories, (s, c) => c.Serialize(s, false));
        stream.WriteList(Texts, (s, str) => s.WriteUtf8String(str));
    }
}