using DisasterPR.Extensions;

namespace DisasterPR.Sessions;

public class CountdownTimeSet
{
    public static readonly List<CountdownTimeSet> TimeSets =
        Enumerable.Range(1, 12).Select(n => new CountdownTimeSet(n * 5)).ToList();

    public static CountdownTimeSet Default => TimeSets[2];  // 15, 45, 90
    
    /// <summary>
    /// The time in seconds a player has to choose a topic.
    /// </summary>
    public int TopicChooseTime { get; }
    
    /// <summary>
    /// The time in seconds a player has to choose an answer in random cards.
    /// </summary>
    public int AnswerChooseTime { get; }
    
    /// <summary>
    /// The time in seconds a player has to choose a final answer from player-chosen cards.
    /// </summary>
    public int FinalChooseTime { get; }
    
    private CountdownTimeSet(int topicChoose, int answerChoose, int finalChoose)
    {
        TopicChooseTime = topicChoose;
        AnswerChooseTime = answerChoose;
        FinalChooseTime = finalChoose;
    }

    private CountdownTimeSet(int topicChoose)
    {
        TopicChooseTime = topicChoose;
        AnswerChooseTime = topicChoose * 3;
        FinalChooseTime = topicChoose * 6;
    }

    public void Serialize(Stream stream)
    {
        stream.WriteVarInt(TopicChooseTime);
        stream.WriteVarInt(AnswerChooseTime);
        stream.WriteVarInt(FinalChooseTime);
    }

    public static CountdownTimeSet Deserialize(Stream stream)
    {
        return new CountdownTimeSet(
            stream.ReadVarInt(),
            stream.ReadVarInt(),
            stream.ReadVarInt()
        );
    }
}