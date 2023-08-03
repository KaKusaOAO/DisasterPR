using System.Diagnostics;
using DisasterPR.Extensions;

namespace DisasterPR.Server;

public static class PlayerName
{
    private static readonly List<string> _adjectives = new();
    private static readonly List<string> _implicitAdjectives = new();
    private static readonly List<string> _nouns = new();
    
    public static bool IsValid(string name)
    {
        if (IsReplacedByRandomName(name))
        {
            // It is unlikely that the generated name is invalid.
            // If in any case it is invalid (ex. too long), ignore that.
            return true;
        }
        
        var processed = ProcessName(name);
        return processed.Length is >= 1 and <= 16;
    }
    
    public static bool IsReplacedByRandomName(string name)
    {
        return name.Trim().All(c => c == '.');
    }
    
    public static string ProcessName(string name)
    {
        if (IsReplacedByRandomName(name))
        {
            return GenerateRandomName();
        }
        
        // Discard the leading and trailing space characters.
        return name.Trim();
    }

    static PlayerName()
    {
        InitVocabularies();
    }

    private static void InitVocabularies()
    {
        _adjectives.Clear();
        _implicitAdjectives.Clear();
        _nouns.Clear();
        
        _adjectives.AddRange(new []
        {
            "自信的", "可愛的", "衝動的", "熱情的", "狂暴的", "狂熱的", "狂野的", "狡猾的", "狡詐的",
            "搞笑的", "搖滾的", "搖擺的", "狂喜的", "歡樂的", "快樂的", "猥瑣的", "色色的", "沉思的", "沉默的",
            "沉穩的", "幼稚的", "膽小的", "勇敢的", "大膽的", "改變世界的", "改變命運的", "改變人生的",
            "香香的", "臭臭的", "受受的", "極致的", "憤怒的", "極速的", "緩慢的",
            "遲鈍的", "煉銅的", "爆肝的", "變態的", "笨拙的", "聰明的", "理智的", "熟練的"
        }.ToHashSet());
        
        // Implicit adjectives are adjectives without the explicit adjective suffix (的).
        _implicitAdjectives.AddRange(new []
        {
            "粉紅色", "棕色", "白色", "淺綠色", "淺藍色", "酒紅色", "墨綠色", "青色",
            "黑色", "紫色", "洋紅色", "芒果色",
            
            "吃糖系", "清楚系", "齁哩系", "吃土系"
        }.ToHashSet());
        
        _nouns.AddRange(new []
        {
            "墨魚", "貓咪", "小魚", "泡菜", "蛋餅", "鐵板麵",
            "糰子", "帆船", "芋頭", "蝸牛", "涼麵", "甜甜圈", "蜜蜂", "螃蟹", "白糖", "橘子",
            "麻糬", "餅乾", "蛋糕"
        }.ToHashSet());
    }

    public static string GenerateRandomName()
    {
        InitVocabularies();

        var rand = Random.Shared;
        var addAdjective = Random.Shared.NextDouble() < 0.5;
        var addImplicitAdjective = Random.Shared.NextDouble() < 0.5;

        if (!addAdjective && !addImplicitAdjective)
        {
            var flag = rand.Next(2) == 0;
            addAdjective = flag;
            addImplicitAdjective = !flag;
        }
        
        var adjective = addAdjective ? _adjectives.Random() : "";
        var color = addImplicitAdjective ? _implicitAdjectives.Random() : "";
        var noun = _nouns.Random();
        return $"{adjective}{color}{noun}";
    }

    public static string ProcessDiscordName(string name)
    {
        const string normalText =
            "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";
        const string fancyText1 =
            "𝔸,𝔹,ℂ,𝔻,𝔼,𝔽,𝔾,ℍ,𝕀,𝕁,𝕂,𝕃,𝕄,ℕ,𝕆,ℙ,ℚ,ℝ,𝕊,𝕋,𝕌,𝕍,𝕎,𝕏,𝕐,ℤ,𝕒,𝕓,𝕔,𝕕,𝕖,𝕗,𝕘,𝕙,𝕚,𝕛,𝕜,𝕝,𝕞,𝕟,𝕠,𝕡,𝕢,𝕣,𝕤,𝕥,𝕦,𝕧,𝕨,𝕩,𝕪,𝕫";

        var normal = normalText.Split(',');
        var fancy1 = fancyText1.Split(',');

        for (var i = 0; i < normal.Length; i++)
        {
            name = name.Replace(fancy1[i], normal[i]);
        }

        return name;
    }
}