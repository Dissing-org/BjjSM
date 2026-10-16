using BjjSM.Client.Models;
using BjjSM.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BjjSM.Client.Pages;

public partial class SpacedRepetitionPage : ComponentBase
{
    [Inject]
    private ISpacedRepetitionApiClient Api { get; set; } = default!;

    private readonly Guid _academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private SpacedRepetitionOverviewDto? _overview;
    private bool _isLoading = true;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _successMessage;

    private string _activeView = "dashboard";
    private Guid? _activeDeckId;

    private StudyDeckDto? ActiveDeck =>
        _overview?.Decks.FirstOrDefault(d => d.Id == _activeDeckId);

    private CardTemplateDto? ActiveDeckTemplate =>
        _overview?.Templates.FirstOrDefault(t => t.Id == ActiveDeck?.TemplateId);

    private List<FlashcardDto> _sessionQueue = [];
    private int _sessionCurrentIndex;
    private bool _isAnswerRevealed;
    private bool _isCramSession;
    private bool _cramReschedule = false;
    private string _cramTagFilter = "All";
    private int _cramTierFilter = -1;
    private int _cramMaxCards = 25;
    private int _sessionCompletedCount;

    private FlashcardDto? CurrentSessionCard =>
        _sessionCurrentIndex >= 0 && _sessionCurrentIndex < _sessionQueue.Count
            ? _sessionQueue[_sessionCurrentIndex]
            : null;

    private string _browserSearch = string.Empty;
    private string _browserTagFilter = "All";
    private int _browserTierFilter = -1;

    private Guid? _editingCardId;
    private string _editingCardTags = string.Empty;
    private List<EditableCardFieldValue> _editingCardFields = [];
    private int _manualRescheduleDays = 7;

    private bool _showDeckModal;
    private Guid? _editingDeckId;
    private string _deckName = string.Empty;
    private string _deckDescription = string.Empty;
    private string _deckCategory = "Guard Retention";
    private Guid _deckTemplateId;
    private string _optLearningSteps = "1, 10";
    private int _optGraduatingDays = 1;
    private int _optEasyDays = 4;
    private double _optStartingEase = 2.5;
    private double _optMinimumEase = 1.3;
    private double _optEasyBonus = 1.3;
    private double _optHardMultiplier = 1.2;
    private double _optIntervalModifier = 1.0;
    private double _optLapseMultiplier = 0.0;
    private int _optMaximumDays = 365;
    private int _optDailyNewLimit = 20;
    private int _optDailyReviewLimit = 100;

    private Guid? _editingTemplateId;
    private string _templateName = string.Empty;
    private string _templateDescription = string.Empty;
    private List<EditableTemplateField> _templateFields = [];

    private bool _showJsonModal;
    private string _jsonModalTitle = string.Empty;
    private string _jsonPayload = string.Empty;
    private bool _isImportMode;

    public sealed class EditableTemplateField
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Side { get; set; } = 1;
        public int FieldType { get; set; } = 1;
        public bool IsRequired { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public sealed class EditableCardFieldValue
    {
        public Guid? Id { get; set; }
        public Guid FieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public int Side { get; set; }
        public int FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadOverviewAsync();
    }

    private async Task LoadOverviewAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        try
        {
            var res = await Api.GetOverviewAsync(_academyId);
            if (res.IsSuccess)
            {
                _overview = res.Value;
                if (_activeDeckId is null && _overview.Decks.Count > 0)
                {
                    _activeDeckId = _overview.Decks[0].Id;
                }
            }
            else
            {
                _errorMessage = res.Error;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SwitchToDashboard()
    {
        _activeView = "dashboard";
        _errorMessage = null;
        _successMessage = null;
    }

    private void OpenCardBrowser(Guid deckId)
    {
        _activeDeckId = deckId;
        _activeView = "cards";
        _browserSearch = string.Empty;
        _browserTagFilter = "All";
        _browserTierFilter = -1;
        InitializeCardEditor(null);
    }

    private void OpenTemplateManager()
    {
        _activeView = "templates";
        if (_overview?.Templates.Count > 0)
        {
            SelectTemplateForEdit(_overview.Templates[0]);
        }
        else
        {
            StartNewTemplate();
        }
    }

    private void StartDueStudySession(Guid deckId)
    {
        _activeDeckId = deckId;
        var deck = ActiveDeck;
        if (deck is null) return;

        var now = DateTime.UtcNow;
        var dueLearningAndReview = deck.Cards
            .Where(c => c.State != 4 && c.State != 0 && c.DueAt <= now)
            .OrderBy(c => c.DueAt)
            .Take(deck.SchedulingOptions.DailyReviewLimit)
            .ToList();

        var dueNew = deck.Cards
            .Where(c => c.State == 0 && c.DueAt <= now)
            .OrderBy(c => c.DueAt)
            .Take(deck.SchedulingOptions.DailyNewCardsLimit)
            .ToList();

        _sessionQueue = dueLearningAndReview.Concat(dueNew).ToList();
        _sessionCurrentIndex = 0;
        _sessionCompletedCount = 0;
        _isAnswerRevealed = false;
        _isCramSession = false;
        _cramReschedule = true;
        _activeView = "study";
        _errorMessage = null;
        _successMessage = null;
    }

    private void OpenCramBuilder(Guid deckId)
    {
        _activeDeckId = deckId;
        _cramTagFilter = "All";
        _cramTierFilter = -1;
        _cramMaxCards = 25;
        _cramReschedule = false;
        _activeView = "cram-setup";
        _errorMessage = null;
        _successMessage = null;
    }

    private void LaunchCustomCramSession()
    {
        var deck = ActiveDeck;
        if (deck is null) return;

        var query = deck.Cards.Where(c => c.State != 4);
        if (_cramTagFilter != "All")
        {
            query = query.Where(c => c.Tags.Any(t => string.Equals(t, _cramTagFilter, StringComparison.OrdinalIgnoreCase)));
        }

        if (_cramTierFilter >= 0)
        {
            query = query.Where(c => c.MaturityTier == _cramTierFilter);
        }

        _sessionQueue = query
            .OrderBy(c => c.DueAt)
            .Take(Math.Max(1, _cramMaxCards))
            .ToList();

        _sessionCurrentIndex = 0;
        _sessionCompletedCount = 0;
        _isAnswerRevealed = false;
        _isCramSession = true;
        _activeView = "study";
    }

    private void RevealAnswer()
    {
        _isAnswerRevealed = true;
    }

    private async Task RateCurrentCardAsync(int rating)
    {
        var deck = ActiveDeck;
        var card = CurrentSessionCard;
        if (deck is null || card is null) return;

        _isSaving = true;
        _errorMessage = null;
        try
        {
            var shouldReschedule = !_isCramSession || _cramReschedule;
            var res = await Api.SubmitReviewAsync(deck.Id, card.Id, rating, shouldReschedule);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            _sessionCompletedCount++;
            var updatedCard = res.Value;

            if (rating == 1 && _sessionQueue.Count < 100)
            {
                _sessionQueue.Add(updatedCard);
            }

            _sessionCurrentIndex++;
            _isAnswerRevealed = false;

            if (_sessionCurrentIndex >= _sessionQueue.Count)
            {
                await LoadOverviewAsync();
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    private IEnumerable<string> GetDeckTags(StudyDeckDto? deck)
    {
        if (deck is null) return ["All"];
        return new[] { "All" }.Concat(
            deck.Cards
                .SelectMany(c => c.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t));
    }

    private IReadOnlyList<FlashcardDto> FilteredBrowserCards
    {
        get
        {
            var deck = ActiveDeck;
            if (deck is null) return [];

            var query = deck.Cards.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_browserSearch))
            {
                var term = _browserSearch.Trim();
                query = query.Where(c =>
                    c.FieldValues.Any(v => v.Value.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    c.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (_browserTagFilter != "All")
            {
                query = query.Where(c => c.Tags.Any(t => string.Equals(t, _browserTagFilter, StringComparison.OrdinalIgnoreCase)));
            }

            if (_browserTierFilter >= 0)
            {
                query = query.Where(c => c.MaturityTier == _browserTierFilter);
            }

            return query.OrderBy(c => c.DueAt).ToList();
        }
    }

    private void InitializeCardEditor(FlashcardDto? card)
    {
        var template = ActiveDeckTemplate;
        if (template is null) return;

        if (card is null)
        {
            _editingCardId = null;
            _editingCardTags = string.Empty;
            _editingCardFields = template.Fields
                .OrderBy(f => f.SortOrder)
                .Select(f => new EditableCardFieldValue
                {
                    FieldDefinitionId = f.Id,
                    FieldName = f.Name,
                    Side = f.Side,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    Value = string.Empty,
                    SortOrder = f.SortOrder
                })
                .ToList();
            return;
        }

        _editingCardId = card.Id;
        _editingCardTags = string.Join(", ", card.Tags);
        _editingCardFields = [];

        foreach (var f in template.Fields.OrderBy(x => x.SortOrder))
        {
            var existingVal = card.FieldValues.FirstOrDefault(v =>
                v.FieldDefinitionId == f.Id ||
                string.Equals(v.FieldName, f.Name, StringComparison.OrdinalIgnoreCase));

            _editingCardFields.Add(new EditableCardFieldValue
            {
                Id = existingVal?.Id,
                FieldDefinitionId = f.Id,
                FieldName = f.Name,
                Side = f.Side,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                Value = existingVal?.Value ?? string.Empty,
                SortOrder = f.SortOrder
            });
        }
    }

    private async Task SaveCardAsync()
    {
        var deck = ActiveDeck;
        if (deck is null) return;

        _isSaving = true;
        _errorMessage = null;
        _successMessage = null;
        try
        {
            var tags = _editingCardTags
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            var fieldInputs = _editingCardFields
                .Select(f => new UpsertCardFieldValueInputDto(
                    f.Id,
                    f.FieldDefinitionId,
                    f.FieldName,
                    f.Side,
                    f.FieldType,
                    f.Value,
                    f.SortOrder))
                .ToList();

            var req = new UpsertFlashcardRequestDto(_editingCardId, tags, fieldInputs);
            var res = await Api.UpsertCardAsync(deck.Id, req);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            await LoadOverviewAsync();
            InitializeCardEditor(null);
            _successMessage = "Flashcard saved.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteCardAsync(Guid cardId)
    {
        var deck = ActiveDeck;
        if (deck is null) return;

        _isSaving = true;
        _errorMessage = null;
        try
        {
            var res = await Api.DeleteCardAsync(deck.Id, cardId);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            await LoadOverviewAsync();
            if (_editingCardId == cardId)
            {
                InitializeCardEditor(null);
            }
            _successMessage = "Flashcard removed.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task ExecuteCardLifecycleAsync(Guid cardId, string action, int? days = null)
    {
        var deck = ActiveDeck;
        if (deck is null) return;

        _isSaving = true;
        _errorMessage = null;
        try
        {
            var res = await Api.ExecuteLifecycleActionAsync(deck.Id, cardId, action, days);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            await LoadOverviewAsync();
            _successMessage = $"Card lifecycle updated ({action}).";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void OpenNewDeckModal()
    {
        _editingDeckId = null;
        _deckName = string.Empty;
        _deckDescription = string.Empty;
        _deckCategory = "Guard Retention";
        _deckTemplateId = _overview?.Templates.FirstOrDefault()?.Id ?? Guid.Empty;
        _optLearningSteps = "1, 10";
        _optGraduatingDays = 1;
        _optEasyDays = 4;
        _optStartingEase = 2.5;
        _optMinimumEase = 1.3;
        _optEasyBonus = 1.3;
        _optHardMultiplier = 1.2;
        _optIntervalModifier = 1.0;
        _optLapseMultiplier = 0.0;
        _optMaximumDays = 365;
        _optDailyNewLimit = 20;
        _optDailyReviewLimit = 100;
        _showDeckModal = true;
        _errorMessage = null;
    }

    private void OpenEditDeckModal(StudyDeckDto deck)
    {
        _editingDeckId = deck.Id;
        _deckName = deck.Name;
        _deckDescription = deck.Description;
        _deckCategory = deck.Category;
        _deckTemplateId = deck.TemplateId;

        var o = deck.SchedulingOptions;
        _optLearningSteps = o.LearningStepsMinutes;
        _optGraduatingDays = o.GraduatingIntervalDays;
        _optEasyDays = o.EasyIntervalDays;
        _optStartingEase = o.StartingEaseFactor;
        _optMinimumEase = o.MinimumEaseFactor;
        _optEasyBonus = o.EasyBonusMultiplier;
        _optHardMultiplier = o.HardIntervalMultiplier;
        _optIntervalModifier = o.IntervalModifier;
        _optLapseMultiplier = o.LapseIntervalMultiplier;
        _optMaximumDays = o.MaximumIntervalDays;
        _optDailyNewLimit = o.DailyNewCardsLimit;
        _optDailyReviewLimit = o.DailyReviewLimit;

        _showDeckModal = true;
        _errorMessage = null;
    }

    private async Task SaveDeckModalAsync()
    {
        _isSaving = true;
        _errorMessage = null;
        try
        {
            var optionsDto = new DeckSchedulingOptionsDto(
                _optLearningSteps,
                _optGraduatingDays,
                _optEasyDays,
                _optStartingEase,
                _optMinimumEase,
                _optEasyBonus,
                _optHardMultiplier,
                _optIntervalModifier,
                _optLapseMultiplier,
                _optMaximumDays,
                _optDailyNewLimit,
                _optDailyReviewLimit);

            var req = new UpsertStudyDeckRequestDto(
                _editingDeckId,
                _academyId,
                _deckTemplateId,
                _deckName,
                _deckDescription,
                _deckCategory,
                optionsDto);

            var res = await Api.UpsertDeckAsync(req);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            _showDeckModal = false;
            _activeDeckId = res.Value.Id;
            await LoadOverviewAsync();
            _successMessage = $"Deck '{res.Value.Name}' saved.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteDeckAsync(Guid deckId)
    {
        _isSaving = true;
        _errorMessage = null;
        try
        {
            var res = await Api.DeleteDeckAsync(deckId);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            if (_activeDeckId == deckId)
            {
                _activeDeckId = null;
            }
            await LoadOverviewAsync();
            _successMessage = "Deck deleted.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void StartNewTemplate()
    {
        _editingTemplateId = null;
        _templateName = string.Empty;
        _templateDescription = string.Empty;
        _templateFields =
        [
            new EditableTemplateField { Name = "Position / Prompt", Side = 1, FieldType = 1, IsRequired = true, SortOrder = 1 },
            new EditableTemplateField { Name = "Counter / Details", Side = 2, FieldType = 2, IsRequired = true, SortOrder = 2 }
        ];
    }

    private void SelectTemplateForEdit(CardTemplateDto template)
    {
        _editingTemplateId = template.Id;
        _templateName = template.Name;
        _templateDescription = template.Description;
        _templateFields = template.Fields
            .OrderBy(f => f.SortOrder)
            .Select(f => new EditableTemplateField
            {
                Id = f.Id,
                Name = f.Name,
                Side = f.Side,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                SortOrder = f.SortOrder
            })
            .ToList();
    }

    private void AddTemplateFieldRow(int side)
    {
        var nextOrder = _templateFields.Count > 0 ? _templateFields.Max(f => f.SortOrder) + 1 : 1;
        _templateFields.Add(new EditableTemplateField
        {
            Name = side == 1 ? "Front Field" : "Back Field",
            Side = side,
            FieldType = 1,
            IsRequired = false,
            SortOrder = nextOrder
        });
    }

    private void RemoveTemplateFieldRow(Guid fieldId)
    {
        _templateFields.RemoveAll(f => f.Id == fieldId);
    }

    private async Task SaveTemplateAsync()
    {
        _isSaving = true;
        _errorMessage = null;
        _successMessage = null;
        try
        {
            var ordered = _templateFields
                .Select((f, idx) => new UpsertCardFieldInputDto(f.Id, f.Name, f.Side, f.FieldType, f.IsRequired, idx + 1))
                .ToList();

            var req = new UpsertCardTemplateRequestDto(
                _editingTemplateId,
                _academyId,
                _templateName,
                _templateDescription,
                ordered);

            var res = await Api.UpsertTemplateAsync(req);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            await LoadOverviewAsync();
            SelectTemplateForEdit(res.Value);
            _successMessage = $"Template '{res.Value.Name}' saved.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteTemplateAsync(Guid templateId)
    {
        _isSaving = true;
        _errorMessage = null;
        try
        {
            var res = await Api.DeleteTemplateAsync(templateId);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            await LoadOverviewAsync();
            if (_overview?.Templates.Count > 0)
            {
                SelectTemplateForEdit(_overview.Templates[0]);
            }
            else
            {
                StartNewTemplate();
            }
            _successMessage = "Template deleted.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task ExportDeckAsync(StudyDeckDto deck)
    {
        _errorMessage = null;
        var res = await Api.ExportDeckJsonAsync(deck.Id);
        if (res.IsFailure)
        {
            _errorMessage = res.Error;
            return;
        }

        _jsonModalTitle = $"Export Deck JSON ({deck.Name})";
        _jsonPayload = res.Value;
        _isImportMode = false;
        _showJsonModal = true;
    }

    private void OpenImportJsonModal()
    {
        _jsonModalTitle = "Import Deck JSON Package";
        _jsonPayload = string.Empty;
        _isImportMode = true;
        _showJsonModal = true;
        _errorMessage = null;
    }

    private async Task ConfirmImportJsonAsync()
    {
        _isSaving = true;
        _errorMessage = null;
        try
        {
            var res = await Api.ImportDeckJsonAsync(_jsonPayload);
            if (res.IsFailure)
            {
                _errorMessage = res.Error;
                return;
            }

            _showJsonModal = false;
            _activeDeckId = res.Value.Id;
            await LoadOverviewAsync();
            _successMessage = $"Imported deck '{res.Value.Name}' with {res.Value.TotalCards} cards.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private static string GetMaturityLabel(int tier) => tier switch
    {
        0 => "New",
        1 => "Learning",
        2 => "Young (<21d)",
        3 => "Mature (21d+)",
        4 => "Suspended",
        _ => "New"
    };

    private static string GetMaturityBadgeStyle(int tier) => tier switch
    {
        0 => "background-color: #EBF0FA; color: #20409A; border: 1px solid #20409A;",
        1 => "background-color: #FFFDE6; color: #050505; border: 1px solid #FFEE00;",
        2 => "background-color: #20409A; color: #FFFFFF;",
        3 => "background-color: #050505; color: #FFEE00;",
        4 => "background-color: #E2E8F0; color: #64748B;",
        _ => "background-color: #EBF0FA; color: #20409A;"
    };

    private static string GetPrimaryPromptPreview(FlashcardDto card)
    {
        var firstFront = card.FieldValues
            .Where(v => v.Side == 1 && !string.IsNullOrWhiteSpace(v.Value))
            .OrderBy(v => v.SortOrder)
            .FirstOrDefault();

        return firstFront?.Value ?? "(Empty Front)";
    }

    private static string GetPrimaryBackPreview(FlashcardDto card)
    {
        var firstBack = card.FieldValues
            .Where(v => v.Side == 2 && !string.IsNullOrWhiteSpace(v.Value))
            .OrderBy(v => v.SortOrder)
            .FirstOrDefault();

        return firstBack?.Value ?? "(Empty Back)";
    }
}
