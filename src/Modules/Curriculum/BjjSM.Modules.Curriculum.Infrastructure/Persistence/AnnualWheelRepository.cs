using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class AnnualWheelRepository : IAnnualWheelRepository
{
    private readonly CurriculumDbContext _context;

    public AnnualWheelRepository(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task<AnnualWheel?> GetByYearAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default)
    {
        return await _context.AnnualWheels
            .Include(w => w.Slots)
            .FirstOrDefaultAsync(w => w.AcademyId == academyId && w.ProgramId == programId && w.Year == year, ct);
    }

    public async Task<AnnualWheel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.AnnualWheels
            .Include(w => w.Slots)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task AddAsync(AnnualWheel wheel, CancellationToken ct = default)
    {
        await _context.AnnualWheels.AddAsync(wheel, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AnnualWheel wheel, CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
