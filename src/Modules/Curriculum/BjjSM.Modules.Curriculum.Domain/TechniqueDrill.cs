using BjjSM.SharedKernel;

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
        var normalizedUrl = ValidateReferenceUrl(referenceUrl);

        Id = id;
        Title = title;
        Category = category;
        CoachingNotes = coachingNotes;
        ReferenceUrl = normalizedUrl;
        this.SortOrder = SortOrder;
    }

    public void Update(
        string title,
        TechniqueCategory category,
        string? coachingNotes,
        string? referenceUrl,
        int SortOrder)
    {
        var normalizedUrl = ValidateReferenceUrl(referenceUrl);

        Title = title;
        Category = category;
        CoachingNotes = coachingNotes;
        ReferenceUrl = normalizedUrl;
        this.SortOrder = SortOrder;
    }

    private static string? ValidateReferenceUrl(string? referenceUrl)
    {
        if (string.IsNullOrWhiteSpace(referenceUrl))
        {
            return null;
        }

        var trimmed = referenceUrl.Trim();
        if (!SafeUrlValidator.IsSafeExternalUrl(trimmed))
        {
            throw new ArgumentException("Reference URL must be a valid http:// or https:// URL.", nameof(referenceUrl));
        }

        return trimmed;
    }
}
