using BjjSM.Modules.Curriculum.Domain;
using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Application;

public sealed class AnnualWheelService : IAnnualWheelService
{
    private readonly IAnnualWheelRepository _wheelRepo;
    private readonly ICurriculumThemeRepository _themeRepo;
    private readonly IMonthlyPlanRepository _planRepo;

    public AnnualWheelService(
        IAnnualWheelRepository wheelRepo,
        ICurriculumThemeRepository themeRepo,
        IMonthlyPlanRepository planRepo)
    {
        _wheelRepo = wheelRepo;
        _themeRepo = themeRepo;
        _planRepo = planRepo;
    }

    public async Task<Result<AnnualWheel>> GetOrGenerateWheelAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default)
    {
        var existing = await _wheelRepo.GetByYearAsync(academyId, programId, year, ct);
        if (existing is not null)
            return Result.Success(existing);

        var createResult = AnnualWheel.Create(academyId, programId, year, AnnualWheelPresets.CreateDefaultSlots(year));
        if (createResult.IsFailure)
            return Result.Failure<AnnualWheel>(createResult.Error);

        var wheel = createResult.Value;
        await _wheelRepo.AddAsync(wheel, ct);
        return Result.Success(wheel);
    }

    public async Task<Result<AnnualWheel>> SwapMonthsAsync(Guid wheelId, int monthA, int monthB, CancellationToken ct = default)
    {
        var wheel = await _wheelRepo.GetByIdAsync(wheelId, ct);
        if (wheel is null)
            return Result.Failure<AnnualWheel>("Annual wheel not found.");

        var swapResult = wheel.SwapMonths(monthA, monthB);
        if (swapResult.IsFailure)
            return Result.Failure<AnnualWheel>(swapResult.Error);

        await _wheelRepo.UpdateAsync(wheel, ct);
        return Result.Success(wheel);
    }

    public async Task<Result<AnnualWheel>> AssignThemeAsync(Guid wheelId, int month, Guid themeId, string? customObjective, CancellationToken ct = default)
    {
        var wheel = await _wheelRepo.GetByIdAsync(wheelId, ct);
        if (wheel is null)
            return Result.Failure<AnnualWheel>("Annual wheel not found.");

        var theme = await _themeRepo.GetByIdAsync(themeId, ct);
        if (theme is null)
            return Result.Failure<AnnualWheel>($"Curriculum theme with ID '{themeId}' not found.");

        var assignResult = wheel.AssignTheme(month, theme, customObjective);
        if (assignResult.IsFailure)
            return Result.Failure<AnnualWheel>(assignResult.Error);

        await _wheelRepo.UpdateAsync(wheel, ct);
        return Result.Success(wheel);
    }

    public async Task<Result<MonthlyPlan>> InstantiateMonthPlanAsync(Guid wheelId, int month, Guid instructorId, CancellationToken ct = default)
    {
        if (instructorId == Guid.Empty)
            return Result.Failure<MonthlyPlan>("InstructorId is required.");

        var wheel = await _wheelRepo.GetByIdAsync(wheelId, ct);
        if (wheel is null)
            return Result.Failure<MonthlyPlan>("Annual wheel not found.");

        var slot = wheel.Slots.FirstOrDefault(s => s.Month == month);
        if (slot is null)
            return Result.Failure<MonthlyPlan>($"Month {month} slot does not exist on this annual wheel.");

        var existingPlan = await _planRepo.GetByMonthAsync(wheel.AcademyId, wheel.ProgramId, wheel.Year, month, ct);
        if (existingPlan is not null)
        {
            if (!slot.IsInstantiated || slot.MonthlyPlanId != existingPlan.Id)
            {
                wheel.MarkMonthInstantiated(month, existingPlan.Id);
                await _wheelRepo.UpdateAsync(wheel, ct);
            }
            return Result.Success(existingPlan);
        }

        var createResult = MonthlyPlan.Create(
            wheel.AcademyId,
            wheel.ProgramId,
            wheel.Year,
            month,
            slot.Objective,
            instructorId,
            weekCount: 4);

        if (createResult.IsFailure)
            return Result.Failure<MonthlyPlan>(createResult.Error);

        var plan = createResult.Value;

        plan.UpdateWeekTheme(1, "Fundamentals & Positioning");
        plan.UpdateWeekTheme(2, "Primary Attacks & Submissions");
        plan.UpdateWeekTheme(3, "Defense & Counters");
        plan.UpdateWeekTheme(4, "Situational Sparring & Integration");

        var theme = await _themeRepo.GetByIdAsync(slot.ThemeId, ct);
        if (theme is not null)
        {
            foreach (var drill in theme.DefaultDrills.OrderBy(d => d.TargetWeekNumber).ThenBy(d => d.SortOrder))
            {
                plan.AddTechnique(
                    drill.TargetWeekNumber,
                    new TechniqueDrill(
                        Guid.NewGuid(),
                        drill.Title,
                        drill.Category,
                        drill.CoachingNotes,
                        drill.ReferenceUrl,
                        drill.SortOrder));
            }
        }

        wheel.MarkMonthInstantiated(month, plan.Id);
        await _planRepo.AddAsync(plan, ct);
        await _wheelRepo.UpdateAsync(wheel, ct);

        return Result.Success(plan);
    }
}
