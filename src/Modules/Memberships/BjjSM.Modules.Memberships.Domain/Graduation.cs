namespace BjjSM.Modules.Memberships.Domain;

public sealed class Graduation
{
    public Guid Id { get; }
    public Guid AcademyId { get; }
    public Rank Rank { get; private set; }
    public DateTime AwardedAt { get; private set; }
    public Guid AwardedBy { get; private set; }
    public string? Notes { get; private set; }

    // Required by EF Core for materializing owned navigation types
    private Graduation()
    {
        Rank = default!;
    }

    internal Graduation(Guid id, Guid academyId, Rank rank, DateTime awardedAt, Guid awardedBy, string? notes)
    {
        Id = id;
        AcademyId = academyId;
        Rank = rank;
        AwardedAt = awardedAt;
        AwardedBy = awardedBy;
        Notes = notes;
    }

    internal void UpdateDetails(Rank rank, DateTime awardedAt, Guid awardedBy, string? notes)
    {
        Rank = rank;
        AwardedAt = awardedAt;
        AwardedBy = awardedBy;
        Notes = notes;
    }
}
