namespace BjjSM.Client.Models;

public static class AchievementEngine
{
    public static List<Achievement> GetDefaultCatalog() =>
    [
        new Achievement
        {
            Id = "basic-master",
            Title = "Basic Master",
            Description = "Fuldend det ultimative capstone talent i Basics træet og opnå total positionsdominans.",
            Icon = "🌟",
            Points = 25,
            Category = "Talenter"
        },
        new Achievement
        {
            Id = "mat-sentinel",
            Title = "Mat Sentinel",
            Description = "Invester 5 eller flere point i Guard fundamentals.",
            Icon = "🥋",
            Points = 10,
            Category = "Talenter"
        },
        new Achievement
        {
            Id = "black-belt-path",
            Title = "Black Belt Path",
            Description = "Invester 25 point i alt på tværs af BJJ Basics talenttræet.",
            Icon = "🥋",
            Points = 20,
            Category = "Talenter"
        }
    ];

    public static List<Achievement> EvaluateTalents(
        IReadOnlyList<Achievement> currentAchievements,
        IReadOnlyList<TalentNode> talents,
        TalentNode incrementedTalent)
    {
        var newlyUnlocked = new List<Achievement>();
        if (currentAchievements == null || currentAchievements.Count == 0)
        {
            return newlyUnlocked;
        }

        // 1. Basic Master (Capstone)
        var maxRow = talents.Count > 0 ? talents.Max(t => t.Row) : 6;
        var isCapstone = (incrementedTalent.Id == "basic-master" || incrementedTalent.Row == 6 || incrementedTalent.Row == maxRow) && incrementedTalent.IsMaxed;

        var basicMaster = currentAchievements.FirstOrDefault(a => a.Id == "basic-master");
        if (isCapstone && basicMaster is not null && !basicMaster.IsUnlocked)
        {
            basicMaster.UnlockedAt = DateTime.UtcNow;
            newlyUnlocked.Add(basicMaster);
        }

        // 2. Mat Sentinel (Row 1 >= 5 points)
        var row1Points = talents.Where(t => t.Row == 1).Sum(t => t.CurrentRank);
        var matSentinel = currentAchievements.FirstOrDefault(a => a.Id == "mat-sentinel");
        if (row1Points >= 5 && matSentinel is not null && !matSentinel.IsUnlocked)
        {
            matSentinel.UnlockedAt = DateTime.UtcNow;
            newlyUnlocked.Add(matSentinel);
        }

        // 3. Black Belt Path (Total points in tree >= 25)
        var totalPoints = talents.Sum(t => t.CurrentRank);
        var blackBelt = currentAchievements.FirstOrDefault(a => a.Id == "black-belt-path");
        if (totalPoints >= 25 && blackBelt is not null && !blackBelt.IsUnlocked)
        {
            blackBelt.UnlockedAt = DateTime.UtcNow;
            newlyUnlocked.Add(blackBelt);
        }

        return newlyUnlocked;
    }
}
