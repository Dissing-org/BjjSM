using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class MonthlyPlan
{
    private readonly List<WeeklyFocus> _weeks = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public Guid ProgramId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string MonthlyObjective { get; private set; }
    public PlanStatus Status { get; private set; }
    public Guid CreatedByInstructorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public IReadOnlyCollection<WeeklyFocus> Weeks => _weeks.AsReadOnly();

    // Required by EF Core
    private MonthlyPlan()
    {
        MonthlyObjective = string.Empty;
    }

    private MonthlyPlan(
        Guid id,
        Guid academyId,
        Guid programId,
        int year,
        int month,
        string objective,
        Guid createdByInstructorId,
        int weekCount)
    {
        Id = id;
        AcademyId = academyId;
        ProgramId = programId;
        Year = year;
        Month = month;
        MonthlyObjective = objective;
        CreatedByInstructorId = createdByInstructorId;
        Status = PlanStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        for (int i = 1; i <= weekCount; i++)
        {
            _weeks.Add(new WeeklyFocus(Guid.NewGuid(), i, string.Empty));
        }
    }

    public static Result<MonthlyPlan> Create(
        Guid academyId,
        Guid programId,
        int year,
        int month,
        string objective,
        Guid createdByInstructorId,
        int weekCount = 4)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<MonthlyPlan>("AcademyId is required.");

        if (programId == Guid.Empty)
            return Result.Failure<MonthlyPlan>("ProgramId is required.");

        if (month < 1 || month > 12)
            return Result.Failure<MonthlyPlan>("Month must be between 1 and 12.");

        if (weekCount < 4 || weekCount > 5)
            return Result.Failure<MonthlyPlan>("WeekCount must be either 4 or 5.");

        if (string.IsNullOrWhiteSpace(objective))
            return Result.Failure<MonthlyPlan>("MonthlyObjective is required.");

        var plan = new MonthlyPlan(
            Guid.NewGuid(),
            academyId,
            programId,
            year,
            month,
            objective.Trim(),
            createdByInstructorId,
            weekCount);

        return Result.Success(plan);
    }

    public Result UpdateObjective(string newObjective)
    {
        if (string.IsNullOrWhiteSpace(newObjective))
            return Result.Failure("MonthlyObjective cannot be empty.");

        MonthlyObjective = newObjective.Trim();
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateWeekTheme(int weekNumber, string theme)
    {
        var week = _weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
        if (week is null)
            return Result.Failure($"Week {weekNumber} does not exist in this plan.");

        week.UpdateTheme(theme.Trim());
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddTechnique(int weekNumber, TechniqueDrill technique)
    {
        var week = _weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
        if (week is null)
            return Result.Failure($"Week {weekNumber} does not exist in this plan.");

        week.AddTechnique(technique);
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateTechnique(
        int weekNumber,
        Guid techniqueId,
        string title,
        TechniqueCategory category,
        string? coachingNotes,
        string? referenceUrl,
        int sortOrder)
    {
        var week = _weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
        if (week is null)
            return Result.Failure($"Week {weekNumber} does not exist in this plan.");

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Technique title cannot be empty.");

        var updated = week.UpdateTechnique(techniqueId, title.Trim(), category, coachingNotes?.Trim(), referenceUrl?.Trim(), sortOrder);
        if (!updated)
            return Result.Failure($"Technique with ID '{techniqueId}' does not exist in Week {weekNumber}.");

        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveTechnique(int weekNumber, Guid techniqueId)
    {
        var week = _weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
        if (week is null)
            return Result.Failure($"Week {weekNumber} does not exist in this plan.");

        week.RemoveTechnique(techniqueId);
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Publish(Guid instructorId)
    {
        if (Status == PlanStatus.Archived)
            return Result.Failure("Cannot publish an archived plan.");

        if (_weeks.Any(w => string.IsNullOrWhiteSpace(w.Theme)))
            return Result.Failure("Every week must have a theme defined before publishing.");

        Status = PlanStatus.Published;
        PublishedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Archive()
    {
        Status = PlanStatus.Archived;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
