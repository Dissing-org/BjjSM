using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class MonthlyPlanTests
{
    private readonly Guid _academyId = Guid.NewGuid();
    private readonly Guid _programId = Guid.NewGuid();
    private readonly Guid _instructorId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeWithDefaultWeeksAndDraftStatus()
    {
        // Act
        var result = MonthlyPlan.Create(
            _academyId,
            _programId,
            year: 2026,
            month: 10,
            objective: "Guard Retention and Defensive Framing",
            createdByInstructorId: _instructorId,
            weekCount: 4);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(PlanStatus.Draft, plan.Status);
        Assert.Equal(2026, plan.Year);
        Assert.Equal(10, plan.Month);
        Assert.Equal("Guard Retention and Defensive Framing", plan.MonthlyObjective);
        Assert.Equal(4, plan.Weeks.Count);
        Assert.All(plan.Weeks, w => Assert.Empty(w.Techniques));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_ShouldFail_WhenMonthIsInvalid(int invalidMonth)
    {
        // Act
        var result = MonthlyPlan.Create(
            _academyId,
            _programId,
            year: 2026,
            month: invalidMonth,
            objective: "Invalid Month Plan",
            createdByInstructorId: _instructorId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Month must be between 1 and 12", result.Error);
    }

    [Fact]
    public void AddTechnique_ShouldAppendTechniqueToSpecificWeek()
    {
        // Arrange
        var plan = MonthlyPlan.Create(_academyId, _programId, 2026, 10, "Passing", _instructorId).Value;
        var technique = new TechniqueDrill(
            Guid.NewGuid(),
            "Knee Slice Pass",
            TechniqueCategory.Technique,
            "Focus on underhook and head control",
            "https://youtube.com/watch?v=sample",
            SortOrder: 1);

        // Act
        var result = plan.AddTechnique(weekNumber: 1, technique);

        // Assert
        Assert.True(result.IsSuccess);
        var week1 = plan.Weeks.First(w => w.WeekNumber == 1);
        Assert.Single(week1.Techniques);
        Assert.Equal("Knee Slice Pass", week1.Techniques.First().Title);
    }

    [Fact]
    public void Publish_ShouldFail_WhenWeeksDoNotHaveThemes()
    {
        // Arrange
        var plan = MonthlyPlan.Create(_academyId, _programId, 2026, 10, "Mount Control", _instructorId).Value;

        // Act (Weeks currently have empty themes)
        var publishResult = plan.Publish(_instructorId);

        // Assert
        Assert.False(publishResult.IsSuccess);
        Assert.Contains("Every week must have a theme defined before publishing", publishResult.Error);
    }

    [Fact]
    public void Publish_ShouldTransitionToPublished_WhenValid()
    {
        // Arrange
        var plan = MonthlyPlan.Create(_academyId, _programId, 2026, 10, "Mount Control", _instructorId).Value;
        for (int i = 1; i <= 4; i++)
        {
            plan.UpdateWeekTheme(i, $"Week {i} Theme");
        }

        // Act
        var publishResult = plan.Publish(_instructorId);

        // Assert
        Assert.True(publishResult.IsSuccess);
        Assert.Equal(PlanStatus.Published, plan.Status);
        Assert.NotNull(plan.PublishedAt);
    }

    [Fact]
    public void UpdateTechnique_ShouldModifyTechniqueInWeek()
    {
        // Arrange
        var plan = MonthlyPlan.Create(_academyId, _programId, 2026, 10, "Passing", _instructorId).Value;
        var techId = Guid.NewGuid();
        var original = new TechniqueDrill(techId, "Armbar", TechniqueCategory.Technique, "Old notes", null, 1);
        plan.AddTechnique(1, original);

        // Act
        var result = plan.UpdateTechnique(1, techId, "Armbar from Guard", TechniqueCategory.Technique, "New notes", "https://youtube.com", 1);

        // Assert
        Assert.True(result.IsSuccess);
        var tech = plan.Weeks.First(w => w.WeekNumber == 1).Techniques.First();
        Assert.Equal("Armbar from Guard", tech.Title);
        Assert.Equal("New notes", tech.CoachingNotes);
        Assert.Equal("https://youtube.com", tech.ReferenceUrl);
    }

    [Fact]
    public void RemoveTechnique_ShouldDeleteTechniqueFromWeek()
    {
        // Arrange
        var plan = MonthlyPlan.Create(_academyId, _programId, 2026, 10, "Passing", _instructorId).Value;
        var techId = Guid.NewGuid();
        var tech = new TechniqueDrill(techId, "Armbar", TechniqueCategory.Technique, null, null, 1);
        plan.AddTechnique(1, tech);

        // Act
        var result = plan.RemoveTechnique(1, techId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(plan.Weeks.First(w => w.WeekNumber == 1).Techniques);
    }
}
