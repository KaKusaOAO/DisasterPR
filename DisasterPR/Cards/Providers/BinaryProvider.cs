using DisasterPR.Extensions;

namespace DisasterPR.Cards.Providers;

public class BinaryProvider : IPackProvider
{
    public Stream Stream { get; }

    public BinaryProvider(Stream stream)
    {
        Stream = stream;
    }

    public async Task<CardPackBuilder> MakeBuilderAsync()
    {
        var guid = Stream.ReadGuid();
        if (guid == Guid.Empty) return await new UpstreamPackProvider().MakeBuilderAsync();

        var builder = CardPackBuilder.Create().WithExplicitGuid(guid);
        builder.Categories.AddRange(Stream.ReadList(CardCategory.Deserialize));
        builder.Topics.AddRange(Stream.ReadList(s => TopicCard.Deserialize(builder.Categories, s)));
        builder.Words.AddRange(Stream.ReadList(s => WordCard.Deserialize(builder.Categories, s)));
        return builder;
    }
}