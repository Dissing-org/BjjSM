using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class MonthlyPlanRepository : IMonthlyPlanRepository
{
    private readonly CurriculumDbContext _context;

    public MonthlyPlanRepository(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MonthlyPlans
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Techniques)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<MonthlyPlan?> GetByMonthAsync(Guid academyId, Guid programId, int year, int month, CancellationToken ct = default)
    {
        return await _context.MonthlyPlans
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Techniques)
            .FirstOrDefaultAsync(p => p.AcademyId == academyId &&
                                      p.ProgramId == programId &&
                                      p.Year == year &&
                                      p.Month == month, ct);
    }

    public async Task AddAsync(MonthlyPlan plan, CancellationToken ct = default)
    {
        await _context.MonthlyPlans.AddAsync(plan, ct);
    }

    public Task DeleteAsync(MonthlyPlan plan, CancellationToken ct = default)
    {
        _context.MonthlyPlans.Remove(plan);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
