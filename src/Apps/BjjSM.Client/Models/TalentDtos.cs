namespace BjjSM.Client.Models;

public class TalentNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Col { get; set; }
    public int MaxRank { get; set; }
    public int CurrentRank { get; set; }
    public string DescriptionFormat { get; set; } = string.Empty;
    public int[] RankValues { get; set; } = [];
    public int RequiredPointsInTree { get; set; }
    public string? PrerequisiteTalentId { get; set; }

    public bool IsMaxed => CurrentRank >= MaxRank;
    public bool HasPoints => CurrentRank > 0;

    public string GetCurrentDescription()
    {
        if (CurrentRank <= 0 || RankValues.Length == 0) return string.Empty;
        var idx = Math.Min(CurrentRank - 1, RankValues.Length - 1);
        return string.Format(DescriptionFormat, RankValues[idx]);
    }

    public string GetNextDescription()
    {
        if (IsMaxed || RankValues.Length == 0) return string.Empty;
        var idx = Math.Min(CurrentRank, RankValues.Length - 1);
        return string.Format(DescriptionFormat, RankValues[idx]);
    }
}
