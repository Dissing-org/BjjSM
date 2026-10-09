using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public interface ICurriculumApiClient
{
    Task<MonthlyPlanDto?> GetPlanAsync(Guid academyId, Guid programId, int year, int month, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> CreatePlanAsync(CreateMonthlyPlanRequest request, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> AddTechniqueAsync(Guid planId, int weekNumber, AddTechniqueRequest request, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> UpdateTechniqueAsync(Guid planId, int weekNumber, Guid techniqueId, UpdateTechniqueRequest request, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> DeleteTechniqueAsync(Guid planId, int weekNumber, Guid techniqueId, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> UpdateWeekThemeAsync(Guid planId, int weekNumber, string theme, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> UpdateObjectiveAsync(Guid planId, string objective, CancellationToken ct = default);
    Task<Result<bool>> DeletePlanAsync(Guid planId, CancellationToken ct = default);
    Task<byte[]?> DownloadPlanPdfAsync(Guid planId, CancellationToken ct = default);
}
