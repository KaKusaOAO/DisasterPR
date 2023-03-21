using DisasterPR.Extensions;

namespace DisasterPR.Cards;

public class WordCard
{
    public List<CardCategory> Categories { get; set; }
    public PartOfSpeech PartOfSpeech { get; set; }
    public virtual string Label { get; set; }

    public static WordCard Deserialize(CardPack pack, Stream stream)
    {
        if (pack == null) throw new ArgumentException($"{nameof(pack)} must present!");

        return new WordCard
        {
            Categories = stream.ReadList(s => CardCategory.DeserializeNoLabel(s, pack)),
            PartOfSpeech = (PartOfSpeech) stream.ReadVarInt(),
            Label = stream.ReadUtf8String()
        };
    }

    public void Serialize(Stream stream)
    {
        stream.WriteList(Categories, (s, c) => c.Serialize(s, false));
        stream.WriteVarInt((int) PartOfSpeech);
        stream.WriteUtf8String(Label);
    }
}