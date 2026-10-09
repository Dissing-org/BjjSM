using BjjSM.Modules.Memberships.Application;
using BjjSM.Modules.Memberships.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Memberships.Infrastructure.Persistence;

public sealed class PractitionerRepository : IPractitionerRepository
{
    private readonly MembershipsDbContext _context;

    public PractitionerRepository(MembershipsDbContext context)
    {
        _context = context;
    }

    public async Task<Practitioner?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Practitioners
            .Include(p => p.Graduations)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Practitioner>> GetByAcademyAsync(Guid academyId, CancellationToken ct = default)
    {
        var query = _context.Practitioners.Include(p => p.Graduations).AsQueryable();

        if (academyId != Guid.Empty)
        {
            query = query.Where(p => p.AcademyId == academyId);
        }

        return await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Practitioner practitioner, CancellationToken ct = default)
    {
        await _context.Practitioners.AddAsync(practitioner, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
