using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Cards.Providers;

public class BinaryProvider : IPackProvider
{
    public BufferReader Reader { get; }

    public BinaryProvider(BufferReader reader)
    {
        Reader = reader;
    }

    public BinaryProvider(Stream stream)
    {
        Reader = new BufferReader(stream);
    }

    public CardPackBuilder MakeBuilder()
    {
        var guid = Reader.ReadGuid();
        if (guid == Guid.Empty) return new UpstreamPackProvider().MakeBuilder();

        var builder = CardPackBuilder.Create().WithExplicitGuid(guid);
        builder.Categories.AddRange(Reader.ReadList(CardCategory.Deserialize));
        builder.Topics.AddRange(Reader.ReadList(s => TopicCard.Deserialize(builder.Categories, s)));
        builder.Words.AddRange(Reader.ReadList(s => WordCard.Deserialize(builder.Categories, s)));
        return builder;
    }
}