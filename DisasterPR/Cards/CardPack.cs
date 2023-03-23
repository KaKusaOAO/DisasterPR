using DisasterPR.Cards.Providers;
using DisasterPR.Extensions;

namespace DisasterPR.Cards;

public class CardPack
{
    public Guid Guid { get; private set; }
    public CardCategory[] Categories { get; private set; }
    public TopicCard[] Topics { get; private set; }
    public WordCard[] Words { get; private set; }

    public static Task<CardPack> GetUpstreamAsync() => IPackProvider.Upstream.MakeAsync();

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

    public static CardPack Deserialize(Stream stream)
    {
        var guid = stream.ReadGuid();
        if (guid == Guid.Empty) return GetUpstreamAsync().Result;
        
        var pack = new CardPack
        {
            Guid = guid,
            Categories = stream.ReadList(CardCategory.Deserialize).ToArray()
        };

        pack.Topics = stream.ReadList(s => TopicCard.Deserialize(pack, s)).ToArray();
        pack.Words = stream.ReadList(s => WordCard.Deserialize(pack, s)).ToArray();
        return pack;
    }

    public void Serialize(Stream stream)
    {
        stream.WriteGuid(Guid);
        if (IsUpstream) return;
        
        stream.WriteList(Categories.ToList(), (s, i) => i.Serialize(s));
        stream.WriteList(Topics.ToList(), (s, i) => i.Serialize(s));
        stream.WriteList(Words.ToList(), (s, i) => i.Serialize(s));
    }

    public List<TopicCard> FilteredTopicsByEnabledCategories(List<CardCategory> categories) =>
        Topics.Where(t => t.Categories.All(categories.Contains)).ToList();
    public List<WordCard> FilteredWordsByEnabledCategories(List<CardCategory> categories) =>
        Words.Where(w => w.Categories.All(categories.Contains)).ToList();
    
    public int GetTopicIndex(TopicCard card) => Topics.IndexOf(card);
    public int GetWordIndex(WordCard card) => Words.IndexOf(card);
}