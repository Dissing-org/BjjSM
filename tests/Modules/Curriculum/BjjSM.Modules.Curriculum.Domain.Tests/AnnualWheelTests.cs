using BjjSM.Modules.Curriculum.Domain;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class AnnualWheelTests
{
    private readonly Guid _academyId = Guid.NewGuid();
    private readonly Guid _programId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldSucceed_WhenGivenExactly12ValidSlots()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);

        var result = AnnualWheel.Create(_academyId, _programId, 2026, slots);

        Assert.True(result.IsSuccess);
        var wheel = result.Value;
        Assert.Equal(2026, wheel.Year);
        Assert.Equal(WheelStatus.Draft, wheel.Status);
        Assert.Equal(12, wheel.Slots.Count);
    }

    [Fact]
    public void Create_ShouldFail_WhenSlotCountIsNot12()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026).Take(11).ToList();

        var result = AnnualWheel.Create(_academyId, _programId, 2026, slots);

        Assert.False(result.IsSuccess);
        Assert.Contains("requires exactly 12", result.Error);
    }

    [Fact]
    public void SwapMonths_ShouldSwapThemeAndObjectiveBetweenTwoSlots()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);
        var wheel = AnnualWheel.Create(_academyId, _programId, 2026, slots).Value;

        var slot1OriginalTheme = wheel.Slots.First(s => s.Month == 1).ThemeName;
        var slot2OriginalTheme = wheel.Slots.First(s => s.Month == 2).ThemeName;

        var swapResult = wheel.SwapMonths(1, 2);

        Assert.True(swapResult.IsSuccess);
        Assert.Equal(slot2OriginalTheme, wheel.Slots.First(s => s.Month == 1).ThemeName);
        Assert.Equal(slot1OriginalTheme, wheel.Slots.First(s => s.Month == 2).ThemeName);
    }

    [Fact]
    public void AssignTheme_ShouldUpdateSlotWithNewThemeAndObjective()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);
        var wheel = AnnualWheel.Create(_academyId, _programId, 2026, slots).Value;
        var newTheme = CurriculumThemePresets.All.Last();

        var assignResult = wheel.AssignTheme(1, newTheme, "Custom objective for January");

        Assert.True(assignResult.IsSuccess);
        var slot1 = wheel.Slots.First(s => s.Month == 1);
        Assert.Equal(newTheme.Id, slot1.ThemeId);
        Assert.Equal(newTheme.Name, slot1.ThemeName);
        Assert.Equal("Custom objective for January", slot1.Objective);
    }

    [Fact]
    public void MarkMonthInstantiated_ShouldSetIsInstantiatedAndMonthlyPlanId()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);
        var wheel = AnnualWheel.Create(_academyId, _programId, 2026, slots).Value;
        var planId = Guid.NewGuid();

        var result = wheel.MarkMonthInstantiated(5, planId);

        Assert.True(result.IsSuccess);
        var slot5 = wheel.Slots.First(s => s.Month == 5);
        Assert.True(slot5.IsInstantiated);
        Assert.Equal(planId, slot5.MonthlyPlanId);
    }

    [Fact]
    public void SwapMonths_ShouldFail_WhenSlotIsAlreadyInstantiated()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);
        var wheel = AnnualWheel.Create(_academyId, _programId, 2026, slots).Value;
        wheel.MarkMonthInstantiated(1, Guid.NewGuid());

        var result = wheel.SwapMonths(1, 2);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot swap months that have already been instantiated", result.Error);
    }

    [Fact]
    public void AssignTheme_ShouldFail_WhenSlotIsAlreadyInstantiated()
    {
        var slots = AnnualWheelPresets.CreateDefaultSlots(2026);
        var wheel = AnnualWheel.Create(_academyId, _programId, 2026, slots).Value;
        wheel.MarkMonthInstantiated(3, Guid.NewGuid());
        var theme = CurriculumThemePresets.CreatePresets().First();

        var result = wheel.AssignTheme(3, theme);

        Assert.False(result.IsSuccess);
        Assert.Contains("already instantiated", result.Error);
    }

    [Fact]
    public void Presets_ShouldContain12CuratedThemesWith4DrillsEach()
    {
        var presets = CurriculumThemePresets.All;
        Assert.Equal(12, presets.Count);
        foreach (var theme in presets)
        {
            Assert.Equal(4, theme.DefaultDrills.Count);
            Assert.Contains(theme.DefaultDrills, d => d.TargetWeekNumber == 1);
            Assert.Contains(theme.DefaultDrills, d => d.TargetWeekNumber == 2);
            Assert.Contains(theme.DefaultDrills, d => d.TargetWeekNumber == 3);
            Assert.Contains(theme.DefaultDrills, d => d.TargetWeekNumber == 4);
        }
    }
}
