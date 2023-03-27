using DisasterPR.Cards;
using DisasterPR.Extensions;

namespace DisasterPR.Sessions;

public class SessionOptions
{
    /// <summary>
    /// 玩家是否可以鎖定卡片？
    /// </summary>
    public bool CanLockCards { get; set; }
    
    /// <summary>
    /// 玩家的勝利分數，介於 2~9 之間。
    /// </summary>
    public int WinScore { get; set; } = 5;
    
    /// <summary>
    /// 各階段倒數時間設定。
    /// </summary>
    public CountdownTimeSet CountdownTimeSet { get; set; } = CountdownTimeSet.Default;

    public List<CardCategory> EnabledCategories { get; set; } = new();

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