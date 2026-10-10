namespace BjjSM.Modules.Curriculum.Domain;

public static class AnnualWheelPresets
{
    public static IReadOnlyList<AnnualWheelSlot> CreateDefaultSlots(int year)
    {
        var themes = CurriculumThemePresets.CreatePresets();
        var slots = new List<AnnualWheelSlot>(12);

        for (int month = 1; month <= 12; month++)
        {
            var theme = themes[month - 1];
            slots.Add(new AnnualWheelSlot(
                Guid.NewGuid(),
                month,
                theme.Id,
                theme.Name,
                theme.DefaultObjective,
                isInstantiated: false,
                monthlyPlanId: null));
        }

        return slots;
    }
}
