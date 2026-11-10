using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class FlowchartRepository : IFlowchartRepository
{
    private readonly CurriculumDbContext _context;

    public FlowchartRepository(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Flowchart>> GetAllAsync(Guid academyId, CancellationToken ct = default)
    {
        return await _context.Flowcharts
            .AsNoTracking()
            .Include(f => f.Nodes)
            .Include(f => f.Edges)
            .Where(f => f.AcademyId == academyId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Flowchart?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Flowcharts
            .Include(f => f.Nodes)
            .Include(f => f.Edges)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task AddAsync(Flowchart flowchart, CancellationToken ct = default)
    {
        await _context.Flowcharts.AddAsync(flowchart, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Flowchart flowchart, CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var flowchart = await _context.Flowcharts
            .Include(f => f.Nodes)
            .Include(f => f.Edges)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (flowchart is not null)
        {
            _context.Flowcharts.Remove(flowchart);
            await _context.SaveChangesAsync(ct);
        }
    }
}
