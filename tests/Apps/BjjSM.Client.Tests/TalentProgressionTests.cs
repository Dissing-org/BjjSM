using BjjSM.Client.Models;
using Xunit;

namespace BjjSM.Client.Tests;

public class TalentProgressionTests
{
    private static List<TalentNode> CreateSampleTree()
    {
        return
        [
            new TalentNode
            {
                Id = "r1-t1",
                Name = "Row 1 Talent 1",
                Row = 1,
                Col = 1,
                MaxRank = 5,
                CurrentRank = 0,
                RequiredPointsInTree = 0,
                RankValues = [2, 4, 6, 8, 10],
                DescriptionFormat = "Bonus {0}%"
            },
            new TalentNode
            {
                Id = "r1-t2",
                Name = "Row 1 Talent 2",
                Row = 1,
                Col = 2,
                MaxRank = 5,
                CurrentRank = 0,
                RequiredPointsInTree = 0,
                RankValues = [2, 4, 6, 8, 10],
                DescriptionFormat = "Bonus {0}%"
            },
            new TalentNode
            {
                Id = "r2-t1",
                Name = "Row 2 Talent 1",
                Row = 2,
                Col = 1,
                MaxRank = 3,
                CurrentRank = 0,
                RequiredPointsInTree = 5,
                RankValues = [5, 10, 15],
                DescriptionFormat = "Bonus {0}%"
            },
            new TalentNode
            {
                Id = "r2-t2",
                Name = "Row 2 Talent 2",
                Row = 2,
                Col = 2,
                MaxRank = 3,
                CurrentRank = 0,
                RequiredPointsInTree = 5,
                RankValues = [5, 10, 15],
                DescriptionFormat = "Bonus {0}%",
                PrerequisiteTalentId = "r1-t1"
            },
            new TalentNode
            {
                Id = "r3-t1",
                Name = "Row 3 Talent 1",
                Row = 3,
                Col = 1,
                MaxRank = 3,
                CurrentRank = 0,
                RequiredPointsInTree = 10,
                RankValues = [5, 10, 15],
                DescriptionFormat = "Bonus {0}%"
            }
        ];
    }

    [Fact]
    public void CanIncrement_Row1Talent_WithZeroPoints_ReturnsTrue()
    {
        var tree = CreateSampleTree();
        var node = tree.First(t => t.Id == "r1-t1");

        Assert.True(TalentProgressionEngine.CanIncrement(tree, node));
    }

    [Fact]
    public void CanIncrement_MaxedTalent_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        var node = tree.First(t => t.Id == "r1-t1");
        node.CurrentRank = node.MaxRank;

        Assert.False(TalentProgressionEngine.CanIncrement(tree, node));
    }

    [Fact]
    public void CanIncrement_Row2Talent_WithInsufficientPointsInRow1_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 4;
        var r2Node = tree.First(t => t.Id == "r2-t1");

        Assert.False(TalentProgressionEngine.CanIncrement(tree, r2Node));
    }

    [Fact]
    public void CanIncrement_Row2Talent_WithEnoughPointsInRow1_ReturnsTrue()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5;
        var r2Node = tree.First(t => t.Id == "r2-t1");

        Assert.True(TalentProgressionEngine.CanIncrement(tree, r2Node));
    }

    [Fact]
    public void CanIncrement_TalentWithUnmetPrerequisite_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        // 5 points split between Row 1 talents, but r1-t1 is not maxed (it has 3, r1-t2 has 2)
        tree.First(t => t.Id == "r1-t1").CurrentRank = 3;
        tree.First(t => t.Id == "r1-t2").CurrentRank = 2;

        var r2WithPrereq = tree.First(t => t.Id == "r2-t2");

        Assert.False(TalentProgressionEngine.CanIncrement(tree, r2WithPrereq));
    }

    [Fact]
    public void CanIncrement_TalentWithMetPrerequisite_ReturnsTrue()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5; // Maxed prereq

        var r2WithPrereq = tree.First(t => t.Id == "r2-t2");

        Assert.True(TalentProgressionEngine.CanIncrement(tree, r2WithPrereq));
    }

    [Fact]
    public void CanDecrement_ZeroRankTalent_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        var node = tree.First(t => t.Id == "r1-t1");
        node.CurrentRank = 0;

        Assert.False(TalentProgressionEngine.CanDecrement(tree, node));
    }

    [Fact]
    public void CanDecrement_Row1Talent_WhenNoHigherTierPointsExist_ReturnsTrue()
    {
        var tree = CreateSampleTree();
        var node = tree.First(t => t.Id == "r1-t1");
        node.CurrentRank = 3;

        Assert.True(TalentProgressionEngine.CanDecrement(tree, node));
    }

    [Fact]
    public void CanDecrement_Row1Talent_WhenHigherTierDependsOnPoints_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5;
        tree.First(t => t.Id == "r2-t1").CurrentRank = 1; // Requires 5 points below

        // Decrementing r1-t1 would leave 4 points below row 2, violating required 5 points
        Assert.False(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r1-t1")));
    }

    [Fact]
    public void CanDecrement_Row1Talent_WhenSurplusPointsExist_ReturnsTrue()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5;
        tree.First(t => t.Id == "r1-t2").CurrentRank = 1; // 6 points total in Row 1
        tree.First(t => t.Id == "r2-t1").CurrentRank = 1; // Requires 5 points below

        // Decrementing r1-t2 leaves 5 points in Row 1, which still satisfies Row 2's requirement
        Assert.True(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r1-t2")));
    }

    [Fact]
    public void CanDecrement_PrerequisiteTalent_WhenDependentTalentHasPoints_ReturnsFalse()
    {
        var tree = CreateSampleTree();
        // r1-t1 has 5 (maxed), r1-t2 has 1, r2-t2 (depends on r1-t1) has 1 point
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5;
        tree.First(t => t.Id == "r1-t2").CurrentRank = 1;
        tree.First(t => t.Id == "r2-t2").CurrentRank = 1;

        // Total points below row 2 is 6, so dropping r1-t1 to 4 still leaves 5 points below,
        // BUT r2-t2 requires r1-t1 to be fully maxed (5/5). So decrementing r1-t1 must return false.
        Assert.False(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r1-t1")));
    }

    [Fact]
    public void CanDecrement_MultiRowCascade_ProtectsAllHigherTiers()
    {
        var tree = CreateSampleTree();
        tree.First(t => t.Id == "r1-t1").CurrentRank = 5;
        tree.First(t => t.Id == "r2-t1").CurrentRank = 5; // row 2 has 5 points; total points before row 3 = 10
        tree.First(t => t.Id == "r3-t1").CurrentRank = 1; // requires 10 points below

        // Dropping row 1 talent leaves 4 + 5 = 9 < 10 -> false
        Assert.False(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r1-t1")));

        // Dropping row 2 talent leaves 5 + 4 = 9 < 10 -> false
        Assert.False(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r2-t1")));

        // Dropping row 3 talent is valid because no higher tier depends on row 3
        Assert.True(TalentProgressionEngine.CanDecrement(tree, tree.First(t => t.Id == "r3-t1")));
    }

    [Fact]
    public void TalentNode_GetCurrentDescription_And_GetNextDescription_SafelyHandleNullOrEmptyRankValues()
    {
        var node = new TalentNode
        {
            RankValues = null!,
            CurrentRank = 1,
            MaxRank = 3,
            DescriptionFormat = "Value: {0}"
        };

        Assert.Equal(string.Empty, node.GetCurrentDescription());
        Assert.Equal(string.Empty, node.GetNextDescription());
    }

    [Fact]
    public void TalentNode_DescriptionFormatting_CatchesFormatException_AndFallsBack()
    {
        var node = new TalentNode
        {
            RankValues = [10, 20],
            CurrentRank = 1,
            MaxRank = 2,
            DescriptionFormat = "Bonus {0}% {invalid_placeholder}"
        };

        // Standard string.Format would throw FormatException due to {invalid_placeholder}
        var desc = node.GetCurrentDescription();
        Assert.Contains("10", desc);

        var nextDesc = node.GetNextDescription();
        Assert.Contains("20", nextDesc);
    }

    [Fact]
    public void TalentFormModel_ToTalentNode_EnsuresRankValuesNotNull()
    {
        var form = new TalentFormModel
        {
            Name = "Custom Talent",
            RankValuesRaw = ""
        };

        var node = form.ToTalentNode();
        Assert.NotNull(node.RankValues);
        Assert.NotEmpty(node.RankValues);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=N4uE-4_lB1E", true)]
    [InlineData("https://vimeo.com/123456", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void TalentNode_HasVideo_ReturnsExpected(string? videoUrl, bool expected)
    {
        var node = new TalentNode { VideoUrl = videoUrl };
        Assert.Equal(expected, node.HasVideo);
    }

    [Fact]
    public void TalentFormModel_ToTalentNode_MapsVideoUrl_Correctly()
    {
        var formWithValidUrl = new TalentFormModel
        {
            Name = "Armbar",
            VideoUrl = "  https://www.youtube.com/watch?v=N4uE-4_lB1E   "
        };
        var nodeWithValidUrl = formWithValidUrl.ToTalentNode();
        Assert.Equal("https://www.youtube.com/watch?v=N4uE-4_lB1E", nodeWithValidUrl.VideoUrl);
        Assert.True(nodeWithValidUrl.HasVideo);

        var formWithEmptyUrl = new TalentFormModel
        {
            Name = "Triangle",
            VideoUrl = "   "
        };
        var nodeWithEmptyUrl = formWithEmptyUrl.ToTalentNode();
        Assert.Null(nodeWithEmptyUrl.VideoUrl);
        Assert.False(nodeWithEmptyUrl.HasVideo);

        var formWithNullUrl = new TalentFormModel
        {
            Name = "Kimura",
            VideoUrl = null
        };
        var nodeWithNullUrl = formWithNullUrl.ToTalentNode();
        Assert.Null(nodeWithNullUrl.VideoUrl);
        Assert.False(nodeWithNullUrl.HasVideo);
    }

    [Fact]
    public void TalentFormModel_FromTalentNode_PopulatesVideoUrl()
    {
        var node = new TalentNode
        {
            Id = "test-node",
            Name = "Guillotine",
            VideoUrl = "https://www.youtube.com/watch?v=abc123xyz"
        };

        var form = TalentFormModel.FromTalentNode(node);
        Assert.Equal("https://www.youtube.com/watch?v=abc123xyz", form.VideoUrl);
    }
}
