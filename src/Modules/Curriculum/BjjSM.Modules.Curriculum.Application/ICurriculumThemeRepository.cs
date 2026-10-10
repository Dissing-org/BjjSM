using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface ICurriculumThemeRepository
{
    Task<IReadOnlyList<CurriculumTheme>> GetAllThemesAsync(CancellationToken ct = default);
    Task<CurriculumTheme?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
