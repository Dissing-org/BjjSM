namespace BjjSM.Modules.Curriculum.Domain;

public sealed class AnnualWheelSlot
{
    public Guid Id { get; private set; }
    public int Month { get; private set; }
    public Guid ThemeId { get; private set; }
    public string ThemeName { get; private set; }
    public string Objective { get; private set; }
    public bool IsInstantiated { get; private set; }
    public Guid? MonthlyPlanId { get; private set; }

    private AnnualWheelSlot()
    {
        ThemeName = string.Empty;
        Objective = string.Empty;
    }

    public AnnualWheelSlot(
        Guid id,
        int month,
        Guid themeId,
        string themeName,
        string objective,
        bool isInstantiated = false,
        Guid? monthlyPlanId = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required.", nameof(id));

        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

        if (themeId == Guid.Empty)
            throw new ArgumentException("ThemeId is required.", nameof(themeId));

        if (string.IsNullOrWhiteSpace(themeName))
            throw new ArgumentException("ThemeName is required.", nameof(themeName));

        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("Objective is required.", nameof(objective));

        Id = id;
        Month = month;
        ThemeId = themeId;
        ThemeName = themeName.Trim();
        Objective = objective.Trim();
        IsInstantiated = isInstantiated;
        MonthlyPlanId = monthlyPlanId;
    }

    internal void AssignTheme(Guid themeId, string themeName, string objective)
    {
        ThemeId = themeId;
        ThemeName = themeName.Trim();
        Objective = objective.Trim();
    }

    internal void MarkInstantiated(Guid monthlyPlanId)
    {
        IsInstantiated = true;
        MonthlyPlanId = monthlyPlanId;
    }

    internal void SwapWith(AnnualWheelSlot other)
    {
        var tempThemeId = ThemeId;
        var tempThemeName = ThemeName;
        var tempObjective = Objective;

        ThemeId = other.ThemeId;
        ThemeName = other.ThemeName;
        Objective = other.Objective;

        other.ThemeId = tempThemeId;
        other.ThemeName = tempThemeName;
        other.Objective = tempObjective;
    }
}
