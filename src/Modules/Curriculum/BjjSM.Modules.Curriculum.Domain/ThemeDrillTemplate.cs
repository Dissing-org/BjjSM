namespace BjjSM.Modules.Curriculum.Domain;

public sealed class ThemeDrillTemplate
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public TechniqueCategory Category { get; private set; }
    public string? CoachingNotes { get; private set; }
    public string? ReferenceUrl { get; private set; }
    public int TargetWeekNumber { get; private set; }
    public int SortOrder { get; private set; }

    private ThemeDrillTemplate()
    {
        Title = string.Empty;
    }

    public ThemeDrillTemplate(
        Guid id,
        string title,
        TechniqueCategory category,
        string? coachingNotes,
        string? referenceUrl,
        int targetWeekNumber,
        int sortOrder)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (targetWeekNumber < 1 || targetWeekNumber > 4)
            throw new ArgumentOutOfRangeException(nameof(targetWeekNumber), "TargetWeekNumber must be between 1 and 4.");

        Id = id;
        Title = title.Trim();
        Category = category;
        CoachingNotes = coachingNotes?.Trim();
        ReferenceUrl = referenceUrl?.Trim();
        TargetWeekNumber = targetWeekNumber;
        SortOrder = sortOrder;
    }
}
