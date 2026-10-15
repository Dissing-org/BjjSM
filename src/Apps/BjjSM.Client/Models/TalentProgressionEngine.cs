namespace BjjSM.Client.Models;

public static class TalentProgressionEngine
{
    public static bool CanIncrement(IReadOnlyList<TalentNode> talents, TalentNode talent)
    {
        if (talent.CurrentRank >= talent.MaxRank) return false;
        var totalPointsInvested = talents.Sum(t => t.CurrentRank);
        var pointsBeforeRow = talents.Where(t => t.Row < talent.Row).Sum(t => t.CurrentRank);
        var qualifyingPoints = talent.Row == 1 ? totalPointsInvested : pointsBeforeRow;
        if (qualifyingPoints < talent.RequiredPointsInTree) return false;

        if (!string.IsNullOrEmpty(talent.PrerequisiteTalentId))
        {
            var prereq = talents.FirstOrDefault(t => t.Id == talent.PrerequisiteTalentId);
            if (prereq is null || !prereq.IsMaxed) return false;
        }
        return true;
    }

    public static bool CanDecrement(IReadOnlyList<TalentNode> talents, TalentNode talent)
    {
        if (talent.CurrentRank <= 0) return false;

        var simulated = talents.Select(t => new
        {
            t.Id,
            t.Row,
            t.RequiredPointsInTree,
            t.PrerequisiteTalentId,
            Rank = t.Id == talent.Id ? t.CurrentRank - 1 : t.CurrentRank
        }).ToList();

        foreach (var other in simulated.Where(t => t.Rank > 0))
        {
            if (other.PrerequisiteTalentId == talent.Id)
            {
                var targetPrereq = talents.First(t => t.Id == talent.Id);
                if (simulated.First(t => t.Id == talent.Id).Rank < targetPrereq.MaxRank)
                {
                    return false;
                }
            }
        }

        var maxRowWithPoints = simulated.Where(t => t.Rank > 0).Select(t => t.Row).DefaultIfEmpty(1).Max();
        for (int r = 2; r <= maxRowWithPoints; r++)
        {
            var activeInRow = simulated.Where(t => t.Row == r && t.Rank > 0).ToList();
            if (activeInRow.Count > 0)
            {
                var pointsBelow = simulated.Where(t => t.Row < r).Sum(t => t.Rank);
                if (activeInRow.Any(t => pointsBelow < t.RequiredPointsInTree))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
