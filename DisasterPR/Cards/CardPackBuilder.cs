namespace DisasterPR.Cards;

public class CardPackBuilder
{
    public static CardPackBuilder Create() => new();
    
    public List<CardCategory> Categories { get; } = new();
    public List<TopicCard> Topics { get; } = new();
    public List<WordCard> Words { get; } = new();

    public CardPackBuilder AddCategory(Guid guid, string label) => AddCategory(new CardCategory(guid, label));
    
    public CardPackBuilder AddCategory(string label) => AddCategory(new CardCategory(Guid.NewGuid(), label));

    public CardPackBuilder AddCategory(CardCategory category)
    {
        Categories.Add(category);
        return this;
    }

    public CardPackBuilder AddBuiltinCategories()
    {
        foreach (var category in CardCategory.Builtins)
        {
            AddCategory(category);
        }

        return this;
    }

    public CardPackBuilder AddTopic(TopicCard card)
    {
        Topics.Add(card);
        return this;
    }

    public CardPackBuilder AddTopic(CardCategory category, string prompt)
    {
        var texts = prompt.Split("____").ToList();
        return AddTopic(new TopicCard
        {
            Categories = new List<CardCategory> { category },
            Texts = texts
        });
    }
    
    public CardPackBuilder AddTopic(CardCategory category, IEnumerable<string> prompt) => 
        AddTopic(new[] { category }, prompt);

    public CardPackBuilder AddTopic(IEnumerable<CardCategory> categories, IEnumerable<string> prompt)
    {
        return AddTopic(new TopicCard
        {
            Categories = categories.ToList(),
            Texts = prompt.ToList()
        });
    }

    public CardPackBuilder AddWord(WordCard card)
    {
        Words.Add(card);
        return this;
    }
    
    public CardPackBuilder AddWord(CardCategory category, PartOfSpeech pos, string label) => 
        AddWord(new[] { category }, pos, label);

    public CardPackBuilder AddWord(IEnumerable<CardCategory> categories, PartOfSpeech pos, string label)
    {
        return AddWord(new WordCard
        {
            Categories = categories.ToList(),
            PartOfSpeech = pos,
            Label = label
        });
    }

    public CardPack Build()
    {
        var categoryIdSet = new HashSet<Guid>();
        if (Categories.Any(category => !categoryIdSet.Add(category.Guid)))
        {
            throw new Exception("Repeating category Guid!");
        }
        
        return new(Categories.ToArray(), Topics.ToArray(), Words.ToArray());
    }
}