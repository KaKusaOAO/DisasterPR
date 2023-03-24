using System.Text.Json;
using System.Text.Json.Nodes;

namespace DisasterPR.Cards.Providers;

public interface IPackProvider
{
    public static IPackProvider Upstream => new UpstreamPackProvider();

    public static IPackProvider Default => new ConcatPackProvider(Custom, Upstream);

    public static IPackProvider Custom => new LambdaPackProvider(async () =>
    {
        var http = new HttpClient();
        var request = await http.GetStreamAsync(new Uri($"http://{Constants.ServerHost}/packs/customs.json"));
        var data = (await JsonSerializer.DeserializeAsync<JsonObject>(request))!;

        var category = new CardCategory(Guid.NewGuid(), "投稿內容！");
        var builder = CardPackBuilder.Create().AddCategory(category);

        foreach (var node in data["topics"]!.AsArray())
        {
            var texts = node!["texts"]!.GetValue<string>();
            builder.AddTopic(category, texts);
        }

        foreach (var node in data["words"]!.AsArray())
        {
            var label = node!["label"]!.GetValue<string>();
            var posStr = node!["pos"]!.GetValue<string>();
            var pos = Enum.Parse<PartOfSpeech>(posStr, true);
            builder.AddWord(category, pos, label);
        }

        return builder;
    });

    public CardPackBuilder MakeBuilder();
}