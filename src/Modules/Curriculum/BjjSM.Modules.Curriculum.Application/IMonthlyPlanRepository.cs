using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface IMonthlyPlanRepository
{
    Task<MonthlyPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MonthlyPlan?> GetByMonthAsync(Guid academyId, Guid programId, int year, int month, CancellationToken ct = default);
    Task AddAsync(MonthlyPlan plan, CancellationToken ct = default);
    Task DeleteAsync(MonthlyPlan plan, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
