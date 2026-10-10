using BjjSM.Modules.Curriculum.Domain;
using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Application;

public interface IAnnualWheelService
{
    Task<Result<AnnualWheel>> GetOrGenerateWheelAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default);
    Task<Result<AnnualWheel>> SwapMonthsAsync(Guid wheelId, int monthA, int monthB, CancellationToken ct = default);
    Task<Result<AnnualWheel>> AssignThemeAsync(Guid wheelId, int month, Guid themeId, string? customObjective, CancellationToken ct = default);
    Task<Result<MonthlyPlan>> InstantiateMonthPlanAsync(Guid wheelId, int month, Guid instructorId, CancellationToken ct = default);
}
