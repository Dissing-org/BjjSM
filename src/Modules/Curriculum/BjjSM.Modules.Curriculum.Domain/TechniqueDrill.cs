namespace BjjSM.Modules.Curriculum.Domain;

public sealed class TechniqueDrill
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public TechniqueCategory Category { get; private set; }
    public string? CoachingNotes { get; private set; }
    public string? ReferenceUrl { get; private set; }
    public int SortOrder { get; private set; }

    // Required by EF Core
    private TechniqueDrill()
    {
        Title = string.Empty;
    }

    public TechniqueDrill(
        Guid id,
        string title,
        TechniqueCategory category,
        string? coachingNotes,
        string? referenceUrl,
        int SortOrder)
    {
        Id = id;
        Title = title;
        Category = category;
        CoachingNotes = coachingNotes;
        ReferenceUrl = referenceUrl;
        this.SortOrder = SortOrder;
    }

    public void Update(
        string title,
        TechniqueCategory category,
        string? coachingNotes,
        string? referenceUrl,
        int SortOrder)
    {
        Title = title;
        Category = category;
        CoachingNotes = coachingNotes;
        ReferenceUrl = referenceUrl;
        this.SortOrder = SortOrder;
    }
}
