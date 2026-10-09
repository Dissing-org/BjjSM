using Xunit;

namespace BjjSM.Modules.Memberships.Domain.Tests;

public class PractitionerTests
{
    private readonly Guid _academyId = Guid.NewGuid();
    private readonly Guid _instructorId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeWithInitialRankAndNoGraduations()
    {
        // Arrange
        var initialRank = Rank.CreateAdult(AdultBelt.White, 0).Value;

        // Act
        var practitionerResult = Practitioner.Create(
            _academyId,
            "Gordon",
            "Ryan",
            "gordon@example.com",
            PractitionerRole.Student,
            initialRank);

        // Assert
        Assert.True(practitionerResult.IsSuccess);
        var practitioner = practitionerResult.Value;
        Assert.Equal("Gordon", practitioner.FirstName);
        Assert.Equal("Ryan", practitioner.LastName);
        Assert.Equal(initialRank, practitioner.CurrentRank);
        Assert.Empty(practitioner.Graduations);
    }

    [Fact]
    public void Promote_ShouldAddNewGraduationAndUpdateCurrentRank()
    {
        // Arrange
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;
        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 0).Value;
        var practitioner = Practitioner.Create(_academyId, "Marcus", "Almeida", "buchecha@example.com", PractitionerRole.Student, whiteRank).Value;
        var awardDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var promoteResult = practitioner.Promote(blueRank, awardDate, _instructorId, "Outstanding performance in tournament");

        // Assert
        Assert.True(promoteResult.IsSuccess);
        Assert.Equal(blueRank, practitioner.CurrentRank);
        Assert.Single(practitioner.Graduations);

        var grad = practitioner.Graduations.First();
        Assert.Equal(blueRank, grad.Rank);
        Assert.Equal(awardDate, grad.AwardedAt);
        Assert.Equal(_instructorId, grad.AwardedBy);
        Assert.Equal("Outstanding performance in tournament", grad.Notes);
    }

    [Fact]
    public void Promote_ShouldFail_WhenAwardDateIsInFuture()
    {
        // Arrange
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;
        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 0).Value;
        var practitioner = Practitioner.Create(_academyId, "Roger", "Gracie", "roger@example.com", PractitionerRole.Student, whiteRank).Value;
        var futureDate = DateTime.UtcNow.AddDays(5);

        // Act
        var promoteResult = practitioner.Promote(blueRank, futureDate, _instructorId, "Future promotion");

        // Assert
        Assert.False(promoteResult.IsSuccess);
        Assert.Contains("Award date cannot be in the future", promoteResult.Error);
    }

    [Fact]
    public void CorrectGraduation_ShouldUpdateEntryAndRecalculateCurrentRank()
    {
        // Arrange
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;
        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 0).Value;
        var purpleRank = Rank.CreateAdult(AdultBelt.Purple, 0).Value;
        var practitioner = Practitioner.Create(_academyId, "Marcelo", "Garcia", "marcelo@example.com", PractitionerRole.Student, whiteRank).Value;

        var t1 = DateTime.UtcNow.AddDays(-20);
        var t2 = DateTime.UtcNow.AddDays(-10);

        practitioner.Promote(blueRank, t1, _instructorId, null);
        practitioner.Promote(purpleRank, t2, _instructorId, null);

        var latestGradId = practitioner.Graduations.Last().Id;
        var correctedPurpleRank = Rank.CreateAdult(AdultBelt.Purple, 1).Value;

        // Act: coach corrects purple belt to purple belt with 1 stripe
        var correctResult = practitioner.CorrectGraduation(latestGradId, correctedPurpleRank, t2, _instructorId, "Added missing stripe");

        // Assert
        Assert.True(correctResult.IsSuccess);
        Assert.Equal(correctedPurpleRank, practitioner.CurrentRank);
    }

    [Fact]
    public void RemoveGraduation_ShouldRemoveEntryAndRevertCurrentRankToPrevious()
    {
        // Arrange
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;
        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 0).Value;
        var practitioner = Practitioner.Create(_academyId, "Keenan", "Cornelius", "keenan@example.com", PractitionerRole.Student, whiteRank).Value;

        practitioner.Promote(blueRank, DateTime.UtcNow.AddDays(-5), _instructorId, "Erroneous promotion entry");
        var gradId = practitioner.Graduations.Single().Id;

        // Act: coach deletes accidental promotion
        var removeResult = practitioner.RemoveGraduation(gradId);

        // Assert
        Assert.True(removeResult.IsSuccess);
        Assert.Empty(practitioner.Graduations);
        Assert.Equal(whiteRank, practitioner.CurrentRank);
    }
}
