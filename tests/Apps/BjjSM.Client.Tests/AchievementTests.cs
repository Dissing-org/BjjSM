using BjjSM.Client.Models;
using Xunit;

namespace BjjSM.Client.Tests;

public class AchievementTests
{
    private static List<TalentNode> CreateSampleTree()
    {
        return
        [
            new TalentNode
            {
                Id = "full-guard",
                Name = "Full guard",
                Row = 1,
                Col = 1,
                MaxRank = 5,
                CurrentRank = 0,
                RequiredPointsInTree = 0
            },
            new TalentNode
            {
                Id = "open-guard",
                Name = "Open guard",
                Row = 1,
                Col = 2,
                MaxRank = 5,
                CurrentRank = 0,
                RequiredPointsInTree = 0
            },
            new TalentNode
            {
                Id = "passing-under",
                Name = "Passing Under",
                Row = 2,
                Col = 1,
                MaxRank = 3,
                CurrentRank = 0,
                RequiredPointsInTree = 5
            },
            new TalentNode
            {
                Id = "back",
                Name = "Back",
                Row = 5,
                Col = 2,
                MaxRank = 5,
                CurrentRank = 0,
                RequiredPointsInTree = 20
            },
            new TalentNode
            {
                Id = "basic-master",
                Name = "Basic Master",
                Row = 6,
                Col = 2,
                MaxRank = 1,
                CurrentRank = 0,
                RequiredPointsInTree = 25
            }
        ];
    }

    [Fact]
    public void GetDefaultCatalog_ContainsAllExpectedAchievements()
    {
        // Act
        var catalog = AchievementEngine.GetDefaultCatalog();

        // Assert
        Assert.NotNull(catalog);
        Assert.Equal(3, catalog.Count);
        Assert.Contains(catalog, a => a.Id == "basic-master" && a.Points == 25);
        Assert.Contains(catalog, a => a.Id == "mat-sentinel" && a.Points == 10);
        Assert.Contains(catalog, a => a.Id == "black-belt-path" && a.Points == 20);
        Assert.All(catalog, a => Assert.False(a.IsUnlocked));
    }

    [Fact]
    public void EvaluateTalents_UnlocksBasicMaster_WhenCapstoneTalentIsMaxed()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var capstone = tree.First(t => t.Id == "basic-master");
        capstone.CurrentRank = 1; // Maxed (MaxRank = 1)

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, capstone);

        // Assert
        Assert.Contains(unlocked, a => a.Id == "basic-master");
        var basicMaster = catalog.First(a => a.Id == "basic-master");
        Assert.True(basicMaster.IsUnlocked);
        Assert.NotNull(basicMaster.UnlockedAt);
    }

    [Fact]
    public void EvaluateTalents_DoesNotUnlockBasicMaster_WhenCapstoneNotMaxed()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var capstone = tree.First(t => t.Id == "basic-master");
        capstone.MaxRank = 3;
        capstone.CurrentRank = 2; // Not yet maxed

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, capstone);

        // Assert
        Assert.DoesNotContain(unlocked, a => a.Id == "basic-master");
        var basicMaster = catalog.First(a => a.Id == "basic-master");
        Assert.False(basicMaster.IsUnlocked);
    }

    [Fact]
    public void EvaluateTalents_DoesNotDuplicate_WhenAlreadyUnlocked()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var basicMaster = catalog.First(a => a.Id == "basic-master");
        var originalUnlockTime = DateTime.UtcNow.AddHours(-1);
        basicMaster.UnlockedAt = originalUnlockTime;

        var tree = CreateSampleTree();
        var capstone = tree.First(t => t.Id == "basic-master");
        capstone.CurrentRank = 1;

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, capstone);

        // Assert
        Assert.DoesNotContain(unlocked, a => a.Id == "basic-master");
        Assert.Equal(originalUnlockTime, basicMaster.UnlockedAt);
    }

    [Fact]
    public void EvaluateTalents_UnlocksMatSentinel_WhenGuardPointsReachThreshold()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var fullGuard = tree.First(t => t.Id == "full-guard");
        var openGuard = tree.First(t => t.Id == "open-guard");

        fullGuard.CurrentRank = 3;
        openGuard.CurrentRank = 2; // Total in Row 1 = 5

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, openGuard);

        // Assert
        Assert.Contains(unlocked, a => a.Id == "mat-sentinel");
        var matSentinel = catalog.First(a => a.Id == "mat-sentinel");
        Assert.True(matSentinel.IsUnlocked);
        Assert.NotNull(matSentinel.UnlockedAt);
    }

    [Fact]
    public void EvaluateTalents_DoesNotUnlockMatSentinel_WhenGuardPointsBelow5()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var fullGuard = tree.First(t => t.Id == "full-guard");
        var openGuard = tree.First(t => t.Id == "open-guard");

        fullGuard.CurrentRank = 2;
        openGuard.CurrentRank = 2; // Total in Row 1 = 4 (< 5)

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, openGuard);

        // Assert
        Assert.DoesNotContain(unlocked, a => a.Id == "mat-sentinel");
        var matSentinel = catalog.First(a => a.Id == "mat-sentinel");
        Assert.False(matSentinel.IsUnlocked);
    }

    [Fact]
    public void EvaluateTalents_UnlocksBlackBeltPath_WhenTotalPointsReach25()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var fullGuard = tree.First(t => t.Id == "full-guard");
        var openGuard = tree.First(t => t.Id == "open-guard");
        var passingUnder = tree.First(t => t.Id == "passing-under");
        var back = tree.First(t => t.Id == "back");

        fullGuard.CurrentRank = 5;
        openGuard.CurrentRank = 5;
        passingUnder.CurrentRank = 3;
        back.MaxRank = 15;
        back.CurrentRank = 12; // Total = 5 + 5 + 3 + 12 = 25

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, back);

        // Assert
        Assert.Contains(unlocked, a => a.Id == "black-belt-path");
        var blackBelt = catalog.First(a => a.Id == "black-belt-path");
        Assert.True(blackBelt.IsUnlocked);
        Assert.NotNull(blackBelt.UnlockedAt);
    }

    [Fact]
    public void EvaluateTalents_DoesNotUnlockBlackBeltPath_WhenTotalPointsBelow25()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var fullGuard = tree.First(t => t.Id == "full-guard");
        fullGuard.CurrentRank = 5; // Total = 5 (< 25)

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, fullGuard);

        // Assert
        Assert.DoesNotContain(unlocked, a => a.Id == "black-belt-path");
        var blackBelt = catalog.First(a => a.Id == "black-belt-path");
        Assert.False(blackBelt.IsUnlocked);
    }

    [Fact]
    public void EvaluateTalents_UnlocksMultipleSimultaneously_WhenThresholdsAlign()
    {
        // Arrange
        var catalog = AchievementEngine.GetDefaultCatalog();
        var tree = CreateSampleTree();
        var fullGuard = tree.First(t => t.Id == "full-guard");
        var openGuard = tree.First(t => t.Id == "open-guard");
        var capstone = tree.First(t => t.Id == "basic-master");

        // Set total points to 24 before incrementing capstone
        fullGuard.CurrentRank = 5;
        openGuard.CurrentRank = 5;
        var passingUnder = tree.First(t => t.Id == "passing-under");
        passingUnder.CurrentRank = 3;
        var back = tree.First(t => t.Id == "back");
        back.MaxRank = 12;
        back.CurrentRank = 11;

        // Now capstone becomes 1 -> total becomes 25 AND capstone is maxed
        capstone.CurrentRank = 1;

        // Also mark mat-sentinel as already unlocked so we check the other two
        var matSentinel = catalog.First(a => a.Id == "mat-sentinel");
        matSentinel.UnlockedAt = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var unlocked = AchievementEngine.EvaluateTalents(catalog, tree, capstone);

        // Assert
        Assert.Equal(2, unlocked.Count);
        Assert.Contains(unlocked, a => a.Id == "basic-master");
        Assert.Contains(unlocked, a => a.Id == "black-belt-path");
        Assert.DoesNotContain(unlocked, a => a.Id == "mat-sentinel");
    }
}
