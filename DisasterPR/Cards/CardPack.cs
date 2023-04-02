using DisasterPR.Attributes;
using DisasterPR.Cards.Providers;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Cards;

public class CardPack
{
    public Guid Guid { get; private set; }
    public CardCategory[] Categories { get; private set; }
    public TopicCard[] Topics { get; private set; }
    public WordCard[] Words { get; private set; }

    public static CardPack GetUpstream() => IPackProvider.Upstream.Make();

    public bool IsUpstream => Guid == Guid.Empty;

    public CardPack(CardCategory[] categories, TopicCard[] topics, WordCard[] words)
    {
        Guid = Guid.NewGuid();
        Categories = categories;
        Topics = topics;
        Words = words;
    }

    internal CardPack(Guid guid, CardCategory[] categories, TopicCard[] topics, WordCard[] words)
    {
        Guid = guid;
        Categories = categories;
        Topics = topics;
        Words = words;
    }

    private CardPack()
    {
        
    }

    public static CardPack Deserialize(BufferReader reader)
    {
        var guid = reader.ReadGuid();
        if (guid == Guid.Empty) return GetUpstream();
        
        var pack = new CardPack
        {
            Guid = guid,
            Categories = reader.ReadList(CardCategory.Deserialize).ToArray()
        };

        pack.Topics = reader.ReadList(s => TopicCard.Deserialize(pack, s)).ToArray();
        pack.Words = reader.ReadList(s => WordCard.Deserialize(pack, s)).ToArray();
        return pack;
    }

    public void Serialize(BufferWriter writer)
    {
        writer.WriteGuid(Guid);
        if (IsUpstream) return;
        
        writer.WriteList(Categories.ToList(), (s, i) => i.Serialize(s));
        writer.WriteList(Topics.ToList(), (s, i) => i.Serialize(s));
        writer.WriteList(Words.ToList(), (s, i) => i.Serialize(s));
    }

    public List<TopicCard> FilteredTopicsByEnabledCategories(List<CardCategory> categories) =>
        Topics.Where(t => t.Categories.All(categories.Contains)).ToList();
    public List<WordCard> FilteredWordsByEnabledCategories(List<CardCategory> categories) =>
        Words.Where(w => w.Categories.All(categories.Contains)).ToList();
    
    public int GetTopicIndex(TopicCard card) => Topics.IndexOf(card);
    public int GetWordIndex(WordCard card) => Words.IndexOf(card);
}