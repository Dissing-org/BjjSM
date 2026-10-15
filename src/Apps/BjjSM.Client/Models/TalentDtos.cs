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

public class TalentFormModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🥋";
    public int Row { get; set; } = 1;
    public int Col { get; set; } = 1;
    public int MaxRank { get; set; } = 3;
    public string DescriptionFormat { get; set; } = "Øger kampeffektivitet med {0}%.";
    public string RankValuesRaw { get; set; } = "5, 10, 15";
    public int RequiredPointsInTree { get; set; } = 0;
    public string? PrerequisiteTalentId { get; set; }

    public bool IsNew => string.IsNullOrWhiteSpace(Id);

    public static TalentFormModel FromTalentNode(TalentNode node)
    {
        return new TalentFormModel
        {
            Id = node.Id,
            Name = node.Name,
            Icon = node.Icon,
            Row = node.Row,
            Col = node.Col,
            MaxRank = node.MaxRank,
            DescriptionFormat = node.DescriptionFormat,
            RankValuesRaw = string.Join(", ", node.RankValues ?? []),
            RequiredPointsInTree = node.RequiredPointsInTree,
            PrerequisiteTalentId = node.PrerequisiteTalentId
        };
    }

    public TalentNode ToTalentNode(int existingCurrentRank = 0)
    {
        var parsedRankValues = ParseRankValues(RankValuesRaw, MaxRank);
        var row = Math.Clamp(Row, 1, 6);
        var col = row switch
        {
            1 => Math.Clamp(Col, 1, 2),
            >= 2 and <= 4 => Math.Clamp(Col, 1, 3),
            5 or 6 => 2,
            _ => Math.Clamp(Col, 1, 3)
        };

        return new TalentNode
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N")[..8] : Id,
            Name = Name?.Trim() ?? string.Empty,
            Icon = string.IsNullOrWhiteSpace(Icon) ? "🥋" : Icon.Trim(),
            Row = row,
            Col = col,
            MaxRank = Math.Clamp(MaxRank, 1, 5),
            CurrentRank = Math.Min(existingCurrentRank, Math.Clamp(MaxRank, 1, 5)),
            DescriptionFormat = DescriptionFormat?.Trim() ?? string.Empty,
            RankValues = parsedRankValues,
            RequiredPointsInTree = RequiredPointsInTree,
            PrerequisiteTalentId = PrerequisiteTalentId
        };
    }

    public static int[] ParseRankValues(string? raw, int maxRank)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Enumerable.Range(1, maxRank).Select(i => i * 5).ToArray();
        }

        var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var list = new List<int>();
        foreach (var p in parts)
        {
            if (int.TryParse(p, out var val))
            {
                list.Add(val);
            }
        }

        if (list.Count == 0)
        {
            return Enumerable.Range(1, maxRank).Select(i => i * 5).ToArray();
        }

        while (list.Count < maxRank)
        {
            var step = list.Count >= 2 ? list[^1] - list[^2] : (list.Count == 1 ? list[0] : 5);
            if (step <= 0) step = 5;
            list.Add(list[^1] + step);
        }

        return list.Take(maxRank).ToArray();
    }
}
