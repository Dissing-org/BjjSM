using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface ISpacedRepetitionRepository
{
    Task<IReadOnlyList<CardTemplate>> GetTemplatesAsync(Guid academyId, CancellationToken ct = default);
    Task<CardTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default);
    Task AddTemplateAsync(CardTemplate template, CancellationToken ct = default);
    Task UpdateTemplateAsync(CardTemplate template, CancellationToken ct = default);
    Task DeleteTemplateAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<StudyDeck>> GetDecksAsync(Guid academyId, CancellationToken ct = default);
    Task<StudyDeck?> GetDeckByIdAsync(Guid id, CancellationToken ct = default);
    Task AddDeckAsync(StudyDeck deck, CancellationToken ct = default);
    Task UpdateDeckAsync(StudyDeck deck, CancellationToken ct = default);
    Task DeleteDeckAsync(Guid id, CancellationToken ct = default);

    Task AddReviewLogAsync(ReviewLog log, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewLog>> GetReviewLogsAsync(Guid academyId, DateTime? sinceUtc = null, CancellationToken ct = default);
}
