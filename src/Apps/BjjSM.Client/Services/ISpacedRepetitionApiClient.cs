using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public interface ISpacedRepetitionApiClient
{
    Task<Result<SpacedRepetitionOverviewDto>> GetOverviewAsync(Guid academyId, CancellationToken ct = default);
    Task<Result<CardTemplateDto>> UpsertTemplateAsync(UpsertCardTemplateRequestDto request, CancellationToken ct = default);
    Task<Result> DeleteTemplateAsync(Guid templateId, CancellationToken ct = default);

    Task<Result<StudyDeckDto>> UpsertDeckAsync(UpsertStudyDeckRequestDto request, CancellationToken ct = default);
    Task<Result> DeleteDeckAsync(Guid deckId, CancellationToken ct = default);

    Task<Result<StudyDeckDto>> UpsertCardAsync(Guid deckId, UpsertFlashcardRequestDto request, CancellationToken ct = default);
    Task<Result<StudyDeckDto>> DeleteCardAsync(Guid deckId, Guid cardId, CancellationToken ct = default);

    Task<Result<FlashcardDto>> SubmitReviewAsync(Guid deckId, Guid cardId, int rating, bool reschedule, CancellationToken ct = default);
    Task<Result<FlashcardDto>> ExecuteLifecycleActionAsync(Guid deckId, Guid cardId, string action, int? intervalDays = null, CancellationToken ct = default);

    Task<Result<string>> ExportDeckJsonAsync(Guid deckId, CancellationToken ct = default);
    Task<Result<StudyDeckDto>> ImportDeckJsonAsync(string rawJson, CancellationToken ct = default);
}
