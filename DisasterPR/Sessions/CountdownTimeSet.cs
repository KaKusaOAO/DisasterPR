﻿using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Sessions;

public class CountdownTimeSet
{
    public static readonly List<CountdownTimeSet> TimeSets =
        Enumerable.Range(1, 12).Select(n => new CountdownTimeSet(n * 5)).ToList();

    public static CountdownTimeSet Default => TimeSets.Find(w => w.TopicChooseTime == 15);  // 15, 45, 90
    
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
    
    public CountdownTimeSet(int topicChoose, int answerChoose, int finalChoose)
    {
        TopicChooseTime = topicChoose;
        AnswerChooseTime = answerChoose;
        FinalChooseTime = finalChoose;
    }

    public CountdownTimeSet(int topicChoose)
    {
        TopicChooseTime = topicChoose;
        AnswerChooseTime = topicChoose * 3;
        FinalChooseTime = topicChoose * 6;
    }

    public void Serialize(BufferWriter stream)
    {
        stream.WriteVarInt(TopicChooseTime);
        stream.WriteVarInt(AnswerChooseTime);
        stream.WriteVarInt(FinalChooseTime);
    }

    public JsonObject SerializeToJson()
    {
        return new JsonObject
        {
            ["topic"] = TopicChooseTime,
            ["answer"] = AnswerChooseTime,
            ["final"] = FinalChooseTime
        };
    }

    public static CountdownTimeSet Deserialize(BufferReader stream)
    {
        return new CountdownTimeSet(
            stream.ReadVarInt(),
            stream.ReadVarInt(),
            stream.ReadVarInt()
        );
    }
    
    public static CountdownTimeSet Deserialize(JsonObject obj)
    {
        return new CountdownTimeSet(
            obj["topic"]!.GetValue<int>(),
            obj["answer"]!.GetValue<int>(),
            obj["final"]!.GetValue<int>()
        );
    }
}