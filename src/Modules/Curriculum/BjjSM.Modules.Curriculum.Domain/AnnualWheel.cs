using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class AnnualWheel
{
    private readonly List<AnnualWheelSlot> _slots = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public Guid ProgramId { get; private set; }
    public int Year { get; private set; }
    public WheelStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public IReadOnlyCollection<AnnualWheelSlot> Slots => _slots.AsReadOnly();

    private AnnualWheel()
    {
    }

    private AnnualWheel(Guid id, Guid academyId, Guid programId, int year, IEnumerable<AnnualWheelSlot> slots)
    {
        Id = id;
        AcademyId = academyId;
        ProgramId = programId;
        Year = year;
        Status = WheelStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        _slots.AddRange(slots.OrderBy(s => s.Month));
    }

    public static Result<AnnualWheel> Create(
        Guid academyId,
        Guid programId,
        int year,
        IEnumerable<AnnualWheelSlot> slots)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<AnnualWheel>("AcademyId is required.");

        if (programId == Guid.Empty)
            return Result.Failure<AnnualWheel>("ProgramId is required.");

        if (year < 2000 || year > 2100)
            return Result.Failure<AnnualWheel>("Year must be a valid 4-digit calendar year.");

        ArgumentNullException.ThrowIfNull(slots);
        var slotList = slots.ToList();

        if (slotList.Count != 12)
            return Result.Failure<AnnualWheel>("AnnualWheel requires exactly 12 monthly slots.");

        var distinctMonths = slotList.Select(s => s.Month).Distinct().OrderBy(m => m).ToList();
        if (distinctMonths.Count != 12 || distinctMonths.First() != 1 || distinctMonths.Last() != 12)
            return Result.Failure<AnnualWheel>("AnnualWheel must contain all 12 consecutive months from 1 to 12.");

        return Result.Success(new AnnualWheel(Guid.NewGuid(), academyId, programId, year, slotList));
    }

    public Result SwapMonths(int monthA, int monthB)
    {
        if (monthA < 1 || monthA > 12 || monthB < 1 || monthB > 12)
            return Result.Failure("Month numbers must be between 1 and 12.");

        if (monthA == monthB)
            return Result.Success();

        var slotA = _slots.FirstOrDefault(s => s.Month == monthA);
        var slotB = _slots.FirstOrDefault(s => s.Month == monthB);

        if (slotA is null || slotB is null)
            return Result.Failure($"Slots for month {monthA} or {monthB} could not be found.");

        if (slotA.IsInstantiated || slotB.IsInstantiated)
            return Result.Failure("Cannot swap months that have already been instantiated into active monthly plans.");

        slotA.SwapWith(slotB);
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AssignTheme(int month, CurriculumTheme theme, string? customObjective = null)
    {
        if (month < 1 || month > 12)
            return Result.Failure("Month number must be between 1 and 12.");

        ArgumentNullException.ThrowIfNull(theme);

        var slot = _slots.FirstOrDefault(s => s.Month == month);
        if (slot is null)
            return Result.Failure($"Slot for month {month} not found.");

        if (slot.IsInstantiated)
            return Result.Failure($"Cannot reassign theme for month {month} because it is already instantiated in the operational curriculum planner.");

        var objective = string.IsNullOrWhiteSpace(customObjective)
            ? theme.DefaultObjective
            : customObjective.Trim();

        slot.AssignTheme(theme.Id, theme.Name, objective);
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkMonthInstantiated(int month, Guid monthlyPlanId)
    {
        if (month < 1 || month > 12)
            return Result.Failure("Month number must be between 1 and 12.");

        if (monthlyPlanId == Guid.Empty)
            return Result.Failure("MonthlyPlanId cannot be empty.");

        var slot = _slots.FirstOrDefault(s => s.Month == month);
        if (slot is null)
            return Result.Failure($"Slot for month {month} not found.");

        slot.MarkInstantiated(monthlyPlanId);
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Activate()
    {
        Status = WheelStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Archive()
    {
        Status = WheelStatus.Archived;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
