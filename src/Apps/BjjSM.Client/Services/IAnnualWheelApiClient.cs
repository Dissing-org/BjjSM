using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public interface IAnnualWheelApiClient
{
    Task<Result<AnnualWheelDto>> GetWheelAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default);
    Task<Result<AnnualWheelDto>> SwapMonthsAsync(Guid wheelId, int monthA, int monthB, CancellationToken ct = default);
    Task<Result<AnnualWheelDto>> AssignThemeAsync(Guid wheelId, int month, Guid themeId, string? customObjective, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> InstantiateMonthAsync(Guid wheelId, int month, Guid instructorId, CancellationToken ct = default);
    Task<IReadOnlyList<CurriculumThemeDto>> GetThemesAsync(CancellationToken ct = default);
}
