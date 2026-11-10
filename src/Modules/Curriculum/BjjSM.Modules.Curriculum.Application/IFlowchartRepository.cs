using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface IFlowchartRepository
{
    Task<IReadOnlyList<Flowchart>> GetAllAsync(Guid academyId, CancellationToken ct = default);
    Task<Flowchart?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Flowchart flowchart, CancellationToken ct = default);
    Task UpdateAsync(Flowchart flowchart, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
