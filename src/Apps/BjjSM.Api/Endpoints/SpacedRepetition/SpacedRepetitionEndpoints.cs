using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.SpacedRepetition;

public sealed record CardFieldDefinitionResponse(
    Guid Id,
    string Name,
    int Side,
    int FieldType,
    bool IsRequired,
    int SortOrder);

public sealed record CardTemplateResponse(
    Guid Id,
    Guid AcademyId,
    string Name,
    string Description,
    IReadOnlyList<CardFieldDefinitionResponse> Fields);

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

public sealed record CardFieldValueResponse(
    Guid Id,
    Guid FieldDefinitionId,
    string FieldName,
    int Side,
    int FieldType,
    string Value,
    int SortOrder);

public sealed record IntervalPreviewResponse(
    string Again,
    string Hard,
    string Good,
    string Easy);

public sealed record FlashcardResponse(
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
    IReadOnlyList<CardFieldValueResponse> FieldValues,
    IntervalPreviewResponse IntervalPreview);

public sealed record StudyDeckResponse(
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
    IReadOnlyList<FlashcardResponse> Cards);

public sealed record ForecastDayResponse(
    string DateLabel,
    DateTime DateUtc,
    int DueCount);

public sealed record SpacedRepetitionOverviewResponse(
    int TotalCards,
    int TotalDueNow,
    int ReviewsTodayCount,
    int TotalReviewsAllTime,
    double OverallRetentionPercent,
    IReadOnlyList<ForecastDayResponse> Forecast14Days,
    IReadOnlyList<CardTemplateResponse> Templates,
    IReadOnlyList<StudyDeckResponse> Decks);

public static class SpacedRepetitionMapper
{
    public static readonly Guid DefaultAcademyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static CardTemplateResponse ToResponse(CardTemplate t) =>
        new(
            t.Id,
            t.AcademyId,
            t.Name,
            t.Description,
            t.Fields
                .OrderBy(f => f.SortOrder)
                .Select(f => new CardFieldDefinitionResponse(
                    f.Id,
                    f.Name,
                    (int)f.Side,
                    (int)f.FieldType,
                    f.IsRequired,
                    f.SortOrder))
                .ToList());

    public static DeckSchedulingOptionsDto ToDto(DeckSchedulingOptions o) =>
        new(
            o.LearningStepsMinutes,
            o.GraduatingIntervalDays,
            o.EasyIntervalDays,
            o.StartingEaseFactor,
            o.MinimumEaseFactor,
            o.EasyBonusMultiplier,
            o.HardIntervalMultiplier,
            o.IntervalModifier,
            o.LapseIntervalMultiplier,
            o.MaximumIntervalDays,
            o.DailyNewCardsLimit,
            o.DailyReviewLimit);

    public static FlashcardResponse ToResponse(Flashcard c, DeckSchedulingOptions options, DateTime nowUtc)
    {
        var preview = Sm2Scheduler.PreviewIntervals(c, options, nowUtc);
        return new FlashcardResponse(
            c.Id,
            c.DeckId,
            c.TemplateId,
            (int)c.State,
            (int)c.MaturityTier,
            c.LearningStepIndex,
            c.IntervalDays,
            c.EaseFactor,
            c.Repetitions,
            c.Lapses,
            c.DueAt,
            c.LastReviewedAt,
            c.GetTags(),
            c.FieldValues
                .OrderBy(v => v.SortOrder)
                .Select(v => new CardFieldValueResponse(
                    v.Id,
                    v.FieldDefinitionId,
                    v.FieldName,
                    (int)v.Side,
                    (int)v.FieldType,
                    v.Value,
                    v.SortOrder))
                .ToList(),
            new IntervalPreviewResponse(preview.Again, preview.Hard, preview.Good, preview.Easy));
    }

    public static StudyDeckResponse ToResponse(
        StudyDeck d,
        IReadOnlyDictionary<Guid, string> templateNames,
        DateTime nowUtc)
    {
        var cards = d.Cards
            .OrderBy(c => c.DueAt)
            .Select(c => ToResponse(c, d.SchedulingOptions, nowUtc))
            .ToList();

        var dueNow = d.Cards.Count(c => c.State != CardState.Suspended && c.DueAt <= nowUtc);
        var newCount = d.Cards.Count(c => c.MaturityTier == CardMaturityTier.New);
        var learningCount = d.Cards.Count(c => c.MaturityTier == CardMaturityTier.Learning);
        var youngCount = d.Cards.Count(c => c.MaturityTier == CardMaturityTier.Young);
        var matureCount = d.Cards.Count(c => c.MaturityTier == CardMaturityTier.Mature);
        var suspendedCount = d.Cards.Count(c => c.MaturityTier == CardMaturityTier.Suspended);

        var tName = templateNames.TryGetValue(d.TemplateId, out var found) ? found : "Custom Template";

        return new StudyDeckResponse(
            d.Id,
            d.AcademyId,
            d.TemplateId,
            tName,
            d.Name,
            d.Description,
            d.Category,
            ToDto(d.SchedulingOptions),
            d.Cards.Count,
            dueNow,
            newCount,
            learningCount,
            youngCount,
            matureCount,
            suspendedCount,
            cards);
    }

    public static SpacedRepetitionOverviewResponse BuildOverview(
        IReadOnlyList<CardTemplate> templates,
        IReadOnlyList<StudyDeck> decks,
        IReadOnlyList<ReviewLog> logs,
        DateTime nowUtc)
    {
        var templateMap = templates.ToDictionary(t => t.Id, t => t.Name);
        var deckResponses = decks.Select(d => ToResponse(d, templateMap, nowUtc)).ToList();
        var templateResponses = templates.Select(ToResponse).ToList();

        var allCards = decks.SelectMany(d => d.Cards).ToList();
        var totalCards = allCards.Count;
        var totalDueNow = allCards.Count(c => c.State != CardState.Suspended && c.DueAt <= nowUtc);

        var startOfToday = nowUtc.Date;
        var reviewsToday = logs.Count(r => r.ReviewedAt >= startOfToday);
        var totalReviews = logs.Count;
        var passedReviews = logs.Count(r => r.Rating is ReviewRating.Good or ReviewRating.Easy);
        var retention = totalReviews > 0
            ? Math.Round(passedReviews * 100.0 / totalReviews, 1)
            : 100.0;

        var forecast = new List<ForecastDayResponse>(14);
        for (var i = 0; i < 14; i++)
        {
            var dayStart = startOfToday.AddDays(i);
            var dayEnd = dayStart.AddDays(1);
            int count;
            if (i == 0)
            {
                count = allCards.Count(c => c.State != CardState.Suspended && c.DueAt < dayEnd);
            }
            else
            {
                count = allCards.Count(c => c.State != CardState.Suspended && c.DueAt >= dayStart && c.DueAt < dayEnd);
            }

            var label = i == 0 ? "Today" : i == 1 ? "Tomorrow" : dayStart.ToString("MMM dd");
            forecast.Add(new ForecastDayResponse(label, dayStart, count));
        }

        return new SpacedRepetitionOverviewResponse(
            totalCards,
            totalDueNow,
            reviewsToday,
            totalReviews,
            retention,
            forecast,
            templateResponses,
            deckResponses);
    }
}

public sealed class GetSpacedRepetitionOverviewEndpoint : EndpointWithoutRequest<SpacedRepetitionOverviewResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public GetSpacedRepetitionOverviewEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/spaced-repetition/overview");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var academyId = Query<Guid?>("academyId", isRequired: false) ?? SpacedRepetitionMapper.DefaultAcademyId;
        var nowUtc = DateTime.UtcNow;

        var templates = await _repository.GetTemplatesAsync(academyId, ct);
        var decks = await _repository.GetDecksAsync(academyId, ct);
        var logs = await _repository.GetReviewLogsAsync(academyId, null, ct);

        var response = SpacedRepetitionMapper.BuildOverview(templates, decks, logs, nowUtc);
        await Send.OkAsync(response, ct);
    }
}

public sealed record UpsertCardFieldInput(
    Guid? Id,
    string Name,
    int Side,
    int FieldType,
    bool IsRequired,
    int SortOrder);

public sealed record UpsertCardTemplateRequest(
    Guid? Id,
    Guid? AcademyId,
    string Name,
    string Description,
    List<UpsertCardFieldInput> Fields);

public sealed class UpsertCardTemplateEndpoint : Endpoint<UpsertCardTemplateRequest, CardTemplateResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public UpsertCardTemplateEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/templates");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpsertCardTemplateRequest req, CancellationToken ct)
    {
        var academyId = req.AcademyId is null || req.AcademyId == Guid.Empty
            ? SpacedRepetitionMapper.DefaultAcademyId
            : req.AcademyId.Value;

        var fields = (req.Fields ?? [])
            .Select(f => new CardFieldDefinition(
                f.Id ?? Guid.NewGuid(),
                f.Name,
                (CardFieldSide)f.Side,
                (CardFieldType)f.FieldType,
                f.IsRequired,
                f.SortOrder))
            .ToList();

        if (req.Id.HasValue && req.Id.Value != Guid.Empty)
        {
            var existing = await _repository.GetTemplateByIdAsync(req.Id.Value, ct);
            if (existing is not null)
            {
                var updateRes = existing.Update(req.Name, req.Description, fields);
                if (updateRes.IsFailure)
                {
                    AddError(updateRes.Error);
                    await Send.ErrorsAsync(400, ct);
                    return;
                }

                await _repository.UpdateTemplateAsync(existing, ct);
                await Send.OkAsync(SpacedRepetitionMapper.ToResponse(existing), ct);
                return;
            }
        }

        var createRes = CardTemplate.Create(req.Id ?? Guid.NewGuid(), academyId, req.Name, req.Description, fields);
        if (createRes.IsFailure)
        {
            AddError(createRes.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        await _repository.AddTemplateAsync(createRes.Value, ct);
        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(createRes.Value), ct);
    }
}

public sealed class DeleteCardTemplateEndpoint : EndpointWithoutRequest
{
    private readonly ISpacedRepetitionRepository _repository;

    public DeleteCardTemplateEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/spaced-repetition/templates/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var decks = await _repository.GetDecksAsync(SpacedRepetitionMapper.DefaultAcademyId, ct);
        if (decks.Any(d => d.TemplateId == id))
        {
            AddError("Cannot delete a template that is actively used by one or more study decks.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        await _repository.DeleteTemplateAsync(id, ct);
        await Send.NoContentAsync(ct);
    }
}

public sealed record UpsertStudyDeckRequest(
    Guid? Id,
    Guid? AcademyId,
    Guid TemplateId,
    string Name,
    string Description,
    string Category,
    DeckSchedulingOptionsDto? SchedulingOptions);

public sealed class UpsertStudyDeckEndpoint : Endpoint<UpsertStudyDeckRequest, StudyDeckResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public UpsertStudyDeckEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/decks");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpsertStudyDeckRequest req, CancellationToken ct)
    {
        var academyId = req.AcademyId is null || req.AcademyId == Guid.Empty
            ? SpacedRepetitionMapper.DefaultAcademyId
            : req.AcademyId.Value;

        var options = DeckSchedulingOptions.Default();
        if (req.SchedulingOptions is not null)
        {
            var o = req.SchedulingOptions;
            var optRes = DeckSchedulingOptions.Create(
                o.LearningStepsMinutes,
                o.GraduatingIntervalDays,
                o.EasyIntervalDays,
                o.StartingEaseFactor,
                o.MinimumEaseFactor,
                o.EasyBonusMultiplier,
                o.HardIntervalMultiplier,
                o.IntervalModifier,
                o.LapseIntervalMultiplier,
                o.MaximumIntervalDays,
                o.DailyNewCardsLimit,
                o.DailyReviewLimit);

            if (optRes.IsFailure)
            {
                AddError(optRes.Error);
                await Send.ErrorsAsync(400, ct);
                return;
            }

            options = optRes.Value;
        }

        var templates = await _repository.GetTemplatesAsync(academyId, ct);
        var templateMap = templates.ToDictionary(t => t.Id, t => t.Name);

        if (req.Id.HasValue && req.Id.Value != Guid.Empty)
        {
            var existing = await _repository.GetDeckByIdAsync(req.Id.Value, ct);
            if (existing is not null)
            {
                var metaRes = existing.UpdateMetadata(req.Name, req.Description, req.Category, req.TemplateId);
                if (metaRes.IsFailure)
                {
                    AddError(metaRes.Error);
                    await Send.ErrorsAsync(400, ct);
                    return;
                }

                existing.UpdateSchedulingOptions(options);
                await _repository.UpdateDeckAsync(existing, ct);
                await Send.OkAsync(SpacedRepetitionMapper.ToResponse(existing, templateMap, DateTime.UtcNow), ct);
                return;
            }
        }

        var createRes = StudyDeck.Create(
            req.Id ?? Guid.NewGuid(),
            academyId,
            req.TemplateId,
            req.Name,
            req.Description,
            req.Category,
            options);

        if (createRes.IsFailure)
        {
            AddError(createRes.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        await _repository.AddDeckAsync(createRes.Value, ct);
        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(createRes.Value, templateMap, DateTime.UtcNow), ct);
    }
}

public sealed class DeleteStudyDeckEndpoint : EndpointWithoutRequest
{
    private readonly ISpacedRepetitionRepository _repository;

    public DeleteStudyDeckEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/spaced-repetition/decks/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await _repository.DeleteDeckAsync(id, ct);
        await Send.NoContentAsync(ct);
    }
}

public sealed record UpsertCardFieldValueInput(
    Guid? Id,
    Guid FieldDefinitionId,
    string FieldName,
    int Side,
    int FieldType,
    string Value,
    int SortOrder);

public sealed record UpsertFlashcardRequest(
    Guid? Id,
    List<string>? Tags,
    List<UpsertCardFieldValueInput> FieldValues);

public sealed class UpsertFlashcardEndpoint : Endpoint<UpsertFlashcardRequest, StudyDeckResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public UpsertFlashcardEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/decks/{deckId:guid}/cards");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpsertFlashcardRequest req, CancellationToken ct)
    {
        var deckId = Route<Guid>("deckId");
        var deck = await _repository.GetDeckByIdAsync(deckId, ct);
        if (deck is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var values = (req.FieldValues ?? [])
            .Select(v => new CardFieldValue(
                v.Id ?? Guid.NewGuid(),
                v.FieldDefinitionId,
                v.FieldName,
                (CardFieldSide)v.Side,
                (CardFieldType)v.FieldType,
                v.Value,
                v.SortOrder))
            .ToList();

        if (req.Id.HasValue && req.Id.Value != Guid.Empty && deck.Cards.Any(c => c.Id == req.Id.Value))
        {
            var updateRes = deck.UpdateCard(req.Id.Value, req.Tags, values);
            if (updateRes.IsFailure)
            {
                AddError(updateRes.Error);
                await Send.ErrorsAsync(400, ct);
                return;
            }
        }
        else
        {
            var addRes = deck.AddCard(req.Id ?? Guid.NewGuid(), deck.TemplateId, req.Tags, values, DateTime.UtcNow);
            if (addRes.IsFailure)
            {
                AddError(addRes.Error);
                await Send.ErrorsAsync(400, ct);
                return;
            }
        }

        await _repository.UpdateDeckAsync(deck, ct);
        var templates = await _repository.GetTemplatesAsync(deck.AcademyId, ct);
        var templateMap = templates.ToDictionary(t => t.Id, t => t.Name);

        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(deck, templateMap, DateTime.UtcNow), ct);
    }
}

public sealed class DeleteFlashcardEndpoint : EndpointWithoutRequest<StudyDeckResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public DeleteFlashcardEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/spaced-repetition/decks/{deckId:guid}/cards/{cardId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var deckId = Route<Guid>("deckId");
        var cardId = Route<Guid>("cardId");

        var deck = await _repository.GetDeckByIdAsync(deckId, ct);
        if (deck is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var removeRes = deck.RemoveCard(cardId);
        if (removeRes.IsFailure)
        {
            AddError(removeRes.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        await _repository.UpdateDeckAsync(deck, ct);
        var templates = await _repository.GetTemplatesAsync(deck.AcademyId, ct);
        var templateMap = templates.ToDictionary(t => t.Id, t => t.Name);

        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(deck, templateMap, DateTime.UtcNow), ct);
    }
}

public sealed record SubmitCardReviewRequest(
    int Rating,
    bool Reschedule = true);

public sealed class SubmitCardReviewEndpoint : Endpoint<SubmitCardReviewRequest, FlashcardResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public SubmitCardReviewEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/decks/{deckId:guid}/cards/{cardId:guid}/review");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SubmitCardReviewRequest req, CancellationToken ct)
    {
        var deckId = Route<Guid>("deckId");
        var cardId = Route<Guid>("cardId");

        if (req.Rating < 1 || req.Rating > 4)
        {
            AddError("Rating must be between 1 (Again) and 4 (Easy).");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var deck = await _repository.GetDeckByIdAsync(deckId, ct);
        if (deck is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var reviewRes = deck.RecordReview(cardId, (ReviewRating)req.Rating, nowUtc, req.Reschedule);
        if (reviewRes.IsFailure)
        {
            AddError(reviewRes.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        await _repository.UpdateDeckAsync(deck, ct);
        await _repository.AddReviewLogAsync(reviewRes.Value, ct);

        var updatedCard = deck.Cards.First(c => c.Id == cardId);
        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(updatedCard, deck.SchedulingOptions, nowUtc), ct);
    }
}

public sealed record CardLifecycleRequest(
    string Action,
    int? IntervalDays = null);

public sealed class CardLifecycleEndpoint : Endpoint<CardLifecycleRequest, FlashcardResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public CardLifecycleEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/decks/{deckId:guid}/cards/{cardId:guid}/lifecycle");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CardLifecycleRequest req, CancellationToken ct)
    {
        var deckId = Route<Guid>("deckId");
        var cardId = Route<Guid>("cardId");

        var deck = await _repository.GetDeckByIdAsync(deckId, ct);
        if (deck is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var card = deck.Cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var action = (req.Action ?? string.Empty).Trim().ToLowerInvariant();
        switch (action)
        {
            case "suspend":
                card.Suspend();
                break;
            case "unsuspend":
                card.Unsuspend(nowUtc);
                break;
            case "reset":
                card.ResetProgress(deck.SchedulingOptions.StartingEaseFactor, nowUtc);
                break;
            case "reschedule":
            {
                var days = Math.Max(0, req.IntervalDays ?? 1);
                card.RescheduleManual(days, nowUtc.AddDays(days));
                break;
            }
            default:
                AddError($"Unsupported lifecycle action '{req.Action}'.");
                await Send.ErrorsAsync(400, ct);
                return;
        }

        await _repository.UpdateDeckAsync(deck, ct);
        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(card, deck.SchedulingOptions, nowUtc), ct);
    }
}

public sealed record ExportCardPackageItem(
    List<string> Tags,
    List<UpsertCardFieldValueInput> FieldValues);

public sealed record DeckExportPackage(
    string FormatVersion,
    DateTime ExportedAtUtc,
    CardTemplateResponse Template,
    string DeckName,
    string DeckDescription,
    string DeckCategory,
    DeckSchedulingOptionsDto SchedulingOptions,
    List<ExportCardPackageItem> Cards);

public sealed class ExportStudyDeckEndpoint : EndpointWithoutRequest<DeckExportPackage>
{
    private readonly ISpacedRepetitionRepository _repository;

    public ExportStudyDeckEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/spaced-repetition/decks/{deckId:guid}/export");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var deckId = Route<Guid>("deckId");
        var deck = await _repository.GetDeckByIdAsync(deckId, ct);
        if (deck is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var template = await _repository.GetTemplateByIdAsync(deck.TemplateId, ct);
        if (template is null)
        {
            var templates = await _repository.GetTemplatesAsync(deck.AcademyId, ct);
            template = templates.First();
        }

        var package = new DeckExportPackage(
            "1.0",
            DateTime.UtcNow,
            SpacedRepetitionMapper.ToResponse(template),
            deck.Name,
            deck.Description,
            deck.Category,
            SpacedRepetitionMapper.ToDto(deck.SchedulingOptions),
            deck.Cards
                .Select(c => new ExportCardPackageItem(
                    c.GetTags().ToList(),
                    c.FieldValues
                        .OrderBy(v => v.SortOrder)
                        .Select(v => new UpsertCardFieldValueInput(
                            null,
                            v.FieldDefinitionId,
                            v.FieldName,
                            (int)v.Side,
                            (int)v.FieldType,
                            v.Value,
                            v.SortOrder))
                        .ToList()))
                .ToList());

        await Send.OkAsync(package, ct);
    }
}

public sealed class ImportStudyDeckEndpoint : Endpoint<DeckExportPackage, StudyDeckResponse>
{
    private readonly ISpacedRepetitionRepository _repository;

    public ImportStudyDeckEndpoint(ISpacedRepetitionRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/spaced-repetition/decks/import");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeckExportPackage req, CancellationToken ct)
    {
        var academyId = SpacedRepetitionMapper.DefaultAcademyId;
        var existingTemplates = await _repository.GetTemplatesAsync(academyId, ct);

        CardTemplate targetTemplate;
        var matchedTemplate = existingTemplates.FirstOrDefault(t =>
            t.Id == req.Template.Id ||
            string.Equals(t.Name, req.Template.Name, StringComparison.OrdinalIgnoreCase));

        if (matchedTemplate is not null)
        {
            targetTemplate = matchedTemplate;
        }
        else
        {
            var newTemplateId = Guid.NewGuid();
            var fieldDefs = req.Template.Fields
                .Select(f => new CardFieldDefinition(
                    Guid.NewGuid(),
                    f.Name,
                    (CardFieldSide)f.Side,
                    (CardFieldType)f.FieldType,
                    f.IsRequired,
                    f.SortOrder))
                .ToList();

            var tRes = CardTemplate.Create(newTemplateId, academyId, req.Template.Name, req.Template.Description, fieldDefs);
            if (tRes.IsFailure)
            {
                AddError(tRes.Error);
                await Send.ErrorsAsync(400, ct);
                return;
            }

            targetTemplate = tRes.Value;
            await _repository.AddTemplateAsync(targetTemplate, ct);
        }

        var o = req.SchedulingOptions;
        var optRes = o is not null
            ? DeckSchedulingOptions.Create(
                o.LearningStepsMinutes,
                o.GraduatingIntervalDays,
                o.EasyIntervalDays,
                o.StartingEaseFactor,
                o.MinimumEaseFactor,
                o.EasyBonusMultiplier,
                o.HardIntervalMultiplier,
                o.IntervalModifier,
                o.LapseIntervalMultiplier,
                o.MaximumIntervalDays,
                o.DailyNewCardsLimit,
                o.DailyReviewLimit)
            : BjjSM.SharedKernel.Result.Success(DeckSchedulingOptions.Default());

        var options = optRes.IsSuccess ? optRes.Value : DeckSchedulingOptions.Default();

        var deckRes = StudyDeck.Create(
            Guid.NewGuid(),
            academyId,
            targetTemplate.Id,
            string.IsNullOrWhiteSpace(req.DeckName) ? "Imported BJJ Deck" : req.DeckName,
            req.DeckDescription ?? string.Empty,
            string.IsNullOrWhiteSpace(req.DeckCategory) ? "Imported" : req.DeckCategory,
            options);

        if (deckRes.IsFailure)
        {
            AddError(deckRes.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var deck = deckRes.Value;
        var nowUtc = DateTime.UtcNow;
        foreach (var item in req.Cards ?? [])
        {
            var values = item.FieldValues
                .Select(v => new CardFieldValue(
                    Guid.NewGuid(),
                    v.FieldDefinitionId == Guid.Empty ? Guid.NewGuid() : v.FieldDefinitionId,
                    v.FieldName,
                    (CardFieldSide)v.Side,
                    (CardFieldType)v.FieldType,
                    v.Value,
                    v.SortOrder))
                .ToList();

            deck.AddCard(Guid.NewGuid(), targetTemplate.Id, item.Tags, values, nowUtc);
        }

        await _repository.AddDeckAsync(deck, ct);

        var updatedTemplates = await _repository.GetTemplatesAsync(academyId, ct);
        var templateMap = updatedTemplates.ToDictionary(t => t.Id, t => t.Name);

        await Send.OkAsync(SpacedRepetitionMapper.ToResponse(deck, templateMap, nowUtc), ct);
    }
}
