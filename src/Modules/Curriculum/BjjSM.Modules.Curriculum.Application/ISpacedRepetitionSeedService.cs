using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Application;

public interface ISpacedRepetitionSeedService
{
    Task EnsurePresetsAndBackfillAsync(Guid academyId, CancellationToken ct = default);
    Task<Result> ResetStarterDecksAsync(Guid academyId, CancellationToken ct = default);
}
