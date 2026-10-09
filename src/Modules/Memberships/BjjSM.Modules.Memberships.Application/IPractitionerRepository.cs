using BjjSM.Modules.Memberships.Domain;

namespace BjjSM.Modules.Memberships.Application;

public interface IPractitionerRepository
{
    Task<Practitioner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Practitioner>> GetByAcademyAsync(Guid academyId, CancellationToken ct = default);
    Task AddAsync(Practitioner practitioner, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
