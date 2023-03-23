namespace DisasterPR.Cards.Providers;

public interface IPackProvider
{
    public static IPackProvider Upstream => new UpstreamPackProvider();

    public static IPackProvider Default => AJinPack;

    public static IPackProvider AJinPack => new LambdaPackProvider(async () =>
    {
        await Task.Yield();

        var category = new CardCategory(Guid.NewGuid(), "新投稿內容");
        return CardPackBuilder.Create()
            .AddCategory(category)
            .AddTopic(category, "阿晋開了一家店！叫做____。")
            .AddTopic(category, "阿晋進香團！____無法擋。")
            .AddWord(category, PartOfSpeech.Noun, "Oh, Mama!")
            ;
    });

    public Task<CardPackBuilder> MakeBuilderAsync();
}