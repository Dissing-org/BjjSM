using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class CurriculumThemeRepository : ICurriculumThemeRepository
{
    private readonly CurriculumDbContext _context;

    public CurriculumThemeRepository(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CurriculumTheme>> GetAllThemesAsync(CancellationToken ct = default)
    {
        return await _context.CurriculumThemes
            .Include(t => t.DefaultDrills)
            .ToListAsync(ct);
    }

    public async Task<CurriculumTheme?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CurriculumThemes
            .Include(t => t.DefaultDrills)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
}
