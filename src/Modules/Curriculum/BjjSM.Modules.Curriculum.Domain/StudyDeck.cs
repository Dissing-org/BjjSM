using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class CardFieldValue
{
    public Guid Id { get; private set; }
    public Guid FieldDefinitionId { get; private set; }
    public string FieldName { get; private set; } = string.Empty;
    public CardFieldSide Side { get; private set; }
    public CardFieldType FieldType { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private CardFieldValue()
    {
    }

    public CardFieldValue(
        Guid id,
        Guid fieldDefinitionId,
        string fieldName,
        CardFieldSide side,
        CardFieldType fieldType,
        string value,
        int sortOrder)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FieldDefinitionId = fieldDefinitionId;
        FieldName = (fieldName ?? string.Empty).Trim();
        Side = side;
        FieldType = fieldType;
        Value = (value ?? string.Empty).Trim();
        SortOrder = sortOrder;
    }
}

public sealed class Flashcard
{
    private readonly List<CardFieldValue> _fieldValues = [];

    public Guid Id { get; private set; }
    public Guid DeckId { get; private set; }
    public Guid TemplateId { get; private set; }
    public CardState State { get; private set; }
    public int LearningStepIndex { get; private set; }
    public double IntervalDays { get; private set; }
    public double EaseFactor { get; private set; }
    public int Repetitions { get; private set; }
    public int Lapses { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime? LastReviewedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string TagsRaw { get; private set; } = string.Empty;
    public IReadOnlyCollection<CardFieldValue> FieldValues => _fieldValues.AsReadOnly();

    public CardMaturityTier MaturityTier => State switch
    {
        CardState.Suspended => CardMaturityTier.Suspended,
        CardState.New => CardMaturityTier.New,
        CardState.Learning or CardState.Relearning => CardMaturityTier.Learning,
        CardState.Review when IntervalDays < 21.0 => CardMaturityTier.Young,
        CardState.Review => CardMaturityTier.Mature,
        _ => CardMaturityTier.New
    };

    private Flashcard()
    {
    }

    internal Flashcard(
        Guid id,
        Guid deckId,
        Guid templateId,
        double startingEaseFactor,
        IEnumerable<string>? tags,
        IEnumerable<CardFieldValue> fieldValues,
        DateTime nowUtc)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        DeckId = deckId;
        TemplateId = templateId;
        State = CardState.New;
        LearningStepIndex = 0;
        IntervalDays = 0;
        EaseFactor = startingEaseFactor;
        Repetitions = 0;
        Lapses = 0;
        DueAt = nowUtc;
        CreatedAt = nowUtc;
        TagsRaw = NormalizeTags(tags);
        _fieldValues.AddRange(fieldValues.OrderBy(f => f.SortOrder));
    }

    public IReadOnlyList<string> GetTags()
    {
        if (string.IsNullOrWhiteSpace(TagsRaw))
            return [];

        return TagsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void ApplySchedule(ScheduleTransition transition, DateTime reviewedAtUtc)
    {
        State = transition.NextState;
        LearningStepIndex = transition.NextStepIndex;
        IntervalDays = transition.NextIntervalDays;
        EaseFactor = transition.NextEaseFactor;
        Repetitions = transition.NextRepetitions;
        Lapses = transition.NextLapses;
        DueAt = transition.NextDueAt;
        LastReviewedAt = reviewedAtUtc;
    }

    public void Suspend()
    {
        State = CardState.Suspended;
    }

    public void Unsuspend(DateTime nowUtc)
    {
        if (State != CardState.Suspended)
            return;

        if (Repetitions > 0 || IntervalDays >= 1.0)
        {
            State = CardState.Review;
        }
        else if (LearningStepIndex > 0)
        {
            State = CardState.Learning;
        }
        else
        {
            State = CardState.New;
            DueAt = nowUtc;
        }
    }

    public void ResetProgress(double startingEaseFactor, DateTime nowUtc)
    {
        State = CardState.New;
        LearningStepIndex = 0;
        IntervalDays = 0;
        EaseFactor = startingEaseFactor;
        Repetitions = 0;
        Lapses = 0;
        DueAt = nowUtc;
        LastReviewedAt = null;
    }

    public void RescheduleManual(int intervalDays, DateTime dueAtUtc)
    {
        var clampedDays = Math.Max(0, intervalDays);
        if (clampedDays == 0)
        {
            State = CardState.Learning;
            LearningStepIndex = 0;
            IntervalDays = 0;
            DueAt = dueAtUtc;
            return;
        }

        State = CardState.Review;
        LearningStepIndex = 0;
        IntervalDays = clampedDays;
        Repetitions = Math.Max(1, Repetitions);
        DueAt = dueAtUtc;
    }

    public Result UpdateContent(IEnumerable<string>? tags, IEnumerable<CardFieldValue> fieldValues)
    {
        if (fieldValues is null)
            return Result.Failure("Card field values are required.");

        var list = fieldValues.ToList();
        if (list.Count == 0 || list.All(f => string.IsNullOrWhiteSpace(f.Value)))
            return Result.Failure("At least one card field must contain text.");

        TagsRaw = NormalizeTags(tags);
        _fieldValues.Clear();
        _fieldValues.AddRange(list.OrderBy(f => f.SortOrder));
        return Result.Success();
    }

    private static string NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
            return string.Empty;

        var clean = tags
            .SelectMany(t => (t ?? string.Empty).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join(", ", clean);
    }
}

public sealed class ReviewLog
{
    public Guid Id { get; private set; }
    public Guid DeckId { get; private set; }
    public Guid CardId { get; private set; }
    public ReviewRating Rating { get; private set; }
    public CardState PreviousState { get; private set; }
    public CardState NewState { get; private set; }
    public double IntervalDaysAfter { get; private set; }
    public double EaseFactorAfter { get; private set; }
    public DateTime ReviewedAt { get; private set; }

    private ReviewLog()
    {
    }

    public ReviewLog(
        Guid id,
        Guid deckId,
        Guid cardId,
        ReviewRating rating,
        CardState previousState,
        CardState newState,
        double intervalDaysAfter,
        double easeFactorAfter,
        DateTime reviewedAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        DeckId = deckId;
        CardId = cardId;
        Rating = rating;
        PreviousState = previousState;
        NewState = newState;
        IntervalDaysAfter = intervalDaysAfter;
        EaseFactorAfter = easeFactorAfter;
        ReviewedAt = reviewedAt;
    }
}

public sealed class StudyDeck
{
    private readonly List<Flashcard> _cards = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public Guid TemplateId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DeckSchedulingOptions SchedulingOptions { get; private set; } = DeckSchedulingOptions.Default();
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public IReadOnlyCollection<Flashcard> Cards => _cards.AsReadOnly();

    private StudyDeck()
    {
    }

    private StudyDeck(
        Guid id,
        Guid academyId,
        Guid templateId,
        string name,
        string description,
        string category,
        DeckSchedulingOptions schedulingOptions)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        AcademyId = academyId;
        TemplateId = templateId;
        Name = name.Trim();
        Description = (description ?? string.Empty).Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "General BJJ" : category.Trim();
        SchedulingOptions = schedulingOptions;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public static Result<StudyDeck> Create(
        Guid id,
        Guid academyId,
        Guid templateId,
        string name,
        string description,
        string category,
        DeckSchedulingOptions? schedulingOptions = null)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<StudyDeck>("AcademyId is required.");

        if (templateId == Guid.Empty)
            return Result.Failure<StudyDeck>("TemplateId is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<StudyDeck>("Deck name is required.");

        return Result.Success(new StudyDeck(
            id,
            academyId,
            templateId,
            name,
            description,
            category,
            schedulingOptions ?? DeckSchedulingOptions.Default()));
    }

    public Result UpdateMetadata(string name, string description, string category, Guid templateId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Deck name is required.");

        if (templateId == Guid.Empty)
            return Result.Failure("TemplateId is required.");

        Name = name.Trim();
        Description = (description ?? string.Empty).Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "General BJJ" : category.Trim();
        TemplateId = templateId;
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateSchedulingOptions(DeckSchedulingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        SchedulingOptions = options;
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<Flashcard> AddCard(
        Guid cardId,
        Guid templateId,
        IEnumerable<string>? tags,
        IEnumerable<CardFieldValue> fieldValues,
        DateTime? nowUtc = null)
    {
        if (fieldValues is null)
            return Result.Failure<Flashcard>("Card field values are required.");

        var list = fieldValues.ToList();
        if (list.Count == 0 || list.All(f => string.IsNullOrWhiteSpace(f.Value)))
            return Result.Failure<Flashcard>("At least one card field must contain text.");

        var timestamp = nowUtc ?? DateTime.UtcNow;
        var card = new Flashcard(
            cardId,
            Id,
            templateId == Guid.Empty ? TemplateId : templateId,
            SchedulingOptions.StartingEaseFactor,
            tags,
            list,
            timestamp);

        _cards.Add(card);
        LastModifiedAt = timestamp;
        return Result.Success(card);
    }

    public Result UpdateCard(Guid cardId, IEnumerable<string>? tags, IEnumerable<CardFieldValue> fieldValues)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Failure("Card not found in deck.");

        var res = card.UpdateContent(tags, fieldValues);
        if (res.IsFailure)
            return res;

        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Failure("Card not found in deck.");

        _cards.Remove(card);
        LastModifiedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<ReviewLog> RecordReview(Guid cardId, ReviewRating rating, DateTime nowUtc, bool reschedule = true)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Failure<ReviewLog>("Card not found in deck.");

        var previousState = card.State;
        var transition = Sm2Scheduler.CalculateNextSchedule(card, SchedulingOptions, rating, nowUtc);

        if (reschedule)
        {
            card.ApplySchedule(transition, nowUtc);
            LastModifiedAt = nowUtc;
        }

        var log = new ReviewLog(
            Guid.NewGuid(),
            Id,
            card.Id,
            rating,
            previousState,
            reschedule ? transition.NextState : previousState,
            reschedule ? transition.NextIntervalDays : card.IntervalDays,
            reschedule ? transition.NextEaseFactor : card.EaseFactor,
            nowUtc);

        return Result.Success(log);
    }
}
