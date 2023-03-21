using DisasterPR.Extensions;

namespace DisasterPR.Sessions;

public class SessionOptions
{
    /// <summary>
    /// The score the winner has to reach. Between 2 and 9.
    /// </summary>
    public int WinScore { get; set; } = 5;
    
    /// <summary>
    /// The countdown time set.
    /// </summary>
    public CountdownTimeSet CountdownTimeSet { get; set; } = CountdownTimeSet.Default;

    public void Serialize(Stream stream)
    {
        stream.WriteVarInt(WinScore);
        CountdownTimeSet.Serialize(stream);
    }

    public static SessionOptions Deserialize(Stream stream)
    {
        var winScore = Math.Clamp(stream.ReadVarInt(), 2, 9);
        var timeSet = CountdownTimeSet.Deserialize(stream);

        return new SessionOptions
        {
            WinScore = winScore,
            CountdownTimeSet = timeSet
        };
    }
}