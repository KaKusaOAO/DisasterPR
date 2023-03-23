namespace DisasterPR.Cards.Providers;

public interface IPackProvider
{
    public static IPackProvider Upstream => new UpstreamPackProvider();

    public static IPackProvider Default => new ConcatPackProvider(Upstream, AJinPack);

    public static IPackProvider AJinPack => new LambdaPackProvider(async () =>
    {
        await Task.Yield();

        var category = new CardCategory(Guid.NewGuid(), "新投稿內容");
        return CardPackBuilder.Create()
            .AddCategory(category)
            .AddTopic(category, "這個標籤需要你的投稿！____是我們的傳統！")
            .AddWord(category, PartOfSpeech.Verb, "吃魚喝茶")
            ;
    });

    public Task<CardPackBuilder> MakeBuilderAsync();
}