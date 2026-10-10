using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface IAnnualWheelRepository
{
    Task<AnnualWheel?> GetByYearAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default);
    Task<AnnualWheel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AnnualWheel wheel, CancellationToken ct = default);
    Task UpdateAsync(AnnualWheel wheel, CancellationToken ct = default);
}
