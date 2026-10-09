using Xunit;

namespace BjjSM.Modules.Memberships.Domain.Tests;

public class RankTests
{
    [Fact]
    public void CreateAdult_ShouldSucceed_WhenValidWhiteBeltWithZeroToFourStripes()
    {
        // Arrange & Act
        var rank0 = Rank.CreateAdult(AdultBelt.White, stripes: 0);
        var rank4 = Rank.CreateAdult(AdultBelt.White, stripes: 4);

        // Assert
        Assert.True(rank0.IsSuccess);
        Assert.Equal(AdultBelt.White, rank0.Value.Belt);
        Assert.Equal(0, rank0.Value.Stripes);

        Assert.True(rank4.IsSuccess);
        Assert.Equal(4, rank4.Value.Stripes);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void CreateAdult_ShouldFail_WhenStripesExceedBoundsForStandardBelt(int invalidStripes)
    {
        // Act
        var result = Rank.CreateAdult(AdultBelt.Blue, stripes: invalidStripes);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Stripes must be between 0 and 4", result.Error);
    }

    [Fact]
    public void CreateAdult_ShouldAllowUpToSixDegrees_ForBlackBelt()
    {
        // Act
        var degree6 = Rank.CreateAdult(AdultBelt.Black, stripes: 6);
        var degree7 = Rank.CreateAdult(AdultBelt.Black, stripes: 7);

        // Assert
        Assert.True(degree6.IsSuccess);
        Assert.Equal(6, degree6.Value.Stripes);

        Assert.False(degree7.IsSuccess);
        Assert.Contains("Black belt degrees must be between 0 and 6", degree7.Error);
    }

    [Fact]
    public void CreateKids_ShouldSucceed_WhenValidGreyBelt()
    {
        // Act
        var result = Rank.CreateKids(KidsBelt.Grey, stripes: 3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BeltLadder.Kids, result.Value.Ladder);
        Assert.Equal("Grey", result.Value.BeltName);
        Assert.Equal(3, result.Value.Stripes);
    }

    [Fact]
    public void Rank_ShouldHaveValueEquality_WhenLadderBeltAndStripesMatch()
    {
        // Arrange
        var rank1 = Rank.CreateAdult(AdultBelt.Purple, stripes: 2).Value;
        var rank2 = Rank.CreateAdult(AdultBelt.Purple, stripes: 2).Value;

        // Assert
        Assert.Equal(rank1, rank2);
        Assert.True(rank1 == rank2);
    }
}
