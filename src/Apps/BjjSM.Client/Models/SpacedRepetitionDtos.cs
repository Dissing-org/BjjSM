namespace BjjSM.Client.Models;

public sealed record CardFieldDefinitionDto(
    Guid Id,
    string Name,
    int Side,
    int FieldType,
    bool IsRequired,
    int SortOrder);

public sealed record CardTemplateDto(
    Guid Id,
    Guid AcademyId,
    string Name,
    string Description,
    IReadOnlyList<CardFieldDefinitionDto> Fields);

public sealed record DeckSchedulingOptionsDto(
    string LearningStepsMinutes,
    int GraduatingIntervalDays,
    int EasyIntervalDays,
    double StartingEaseFactor,
    double MinimumEaseFactor,
    double EasyBonusMultiplier,
    double HardIntervalMultiplier,
    double IntervalModifier,
    double LapseIntervalMultiplier,
    int MaximumIntervalDays,
    int DailyNewCardsLimit,
    int DailyReviewLimit);

public sealed record CardFieldValueDto(
    Guid Id,
    Guid FieldDefinitionId,
    string FieldName,
    int Side,
    int FieldType,
    string Value,
    int SortOrder);

public sealed record IntervalPreviewDto(
    string Again,
    string Hard,
    string Good,
    string Easy);

public sealed record FlashcardDto(
    Guid Id,
    Guid DeckId,
    Guid TemplateId,
    int State,
    int MaturityTier,
    int LearningStepIndex,
    double IntervalDays,
    double EaseFactor,
    int Repetitions,
    int Lapses,
    DateTime DueAt,
    DateTime? LastReviewedAt,
    IReadOnlyList<string> Tags,
    IReadOnlyList<CardFieldValueDto> FieldValues,
    IntervalPreviewDto IntervalPreview);

public sealed record StudyDeckDto(
    Guid Id,
    Guid AcademyId,
    Guid TemplateId,
    string TemplateName,
    string Name,
    string Description,
    string Category,
    DeckSchedulingOptionsDto SchedulingOptions,
    int TotalCards,
    int DueNowCount,
    int NewCount,
    int LearningCount,
    int YoungCount,
    int MatureCount,
    int SuspendedCount,
    IReadOnlyList<FlashcardDto> Cards);

public sealed record ForecastDayDto(
    string DateLabel,
    DateTime DateUtc,
    int DueCount);

public sealed record SpacedRepetitionOverviewDto(
    int TotalCards,
    int TotalDueNow,
    int ReviewsTodayCount,
    int TotalReviewsAllTime,
    double OverallRetentionPercent,
    IReadOnlyList<ForecastDayDto> Forecast14Days,
    IReadOnlyList<CardTemplateDto> Templates,
    IReadOnlyList<StudyDeckDto> Decks);

public sealed record UpsertCardFieldInputDto(
    Guid? Id,
    string Name,
    int Side,
    int FieldType,
    bool IsRequired,
    int SortOrder);

public sealed record UpsertCardTemplateRequestDto(
    Guid? Id,
    Guid? AcademyId,
    string Name,
    string Description,
    List<UpsertCardFieldInputDto> Fields);

public sealed record UpsertStudyDeckRequestDto(
    Guid? Id,
    Guid? AcademyId,
    Guid TemplateId,
    string Name,
    string Description,
    string Category,
    DeckSchedulingOptionsDto? SchedulingOptions);

public sealed record UpsertCardFieldValueInputDto(
    Guid? Id,
    Guid FieldDefinitionId,
    string FieldName,
    int Side,
    int FieldType,
    string Value,
    int SortOrder);

public sealed record UpsertFlashcardRequestDto(
    Guid? Id,
    List<string>? Tags,
    List<UpsertCardFieldValueInputDto> FieldValues);

public sealed record SubmitCardReviewRequestDto(
    int Rating,
    bool Reschedule = true);

public sealed record CardLifecycleRequestDto(
    string Action,
    int? IntervalDays = null);

public sealed record ExportCardPackageItemDto(
    List<string> Tags,
    List<UpsertCardFieldValueInputDto> FieldValues);

public sealed record DeckExportPackageDto(
    string FormatVersion,
    DateTime ExportedAtUtc,
    CardTemplateDto Template,
    string DeckName,
    string DeckDescription,
    string DeckCategory,
    DeckSchedulingOptionsDto SchedulingOptions,
    List<ExportCardPackageItemDto> Cards);
