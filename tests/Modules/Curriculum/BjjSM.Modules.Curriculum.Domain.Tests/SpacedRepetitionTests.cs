using BjjSM.Modules.Curriculum.Domain;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class SpacedRepetitionTests
{
    private readonly Guid _academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void CardTemplate_Create_ShouldFail_WhenMissingFrontOrBackField()
    {
        var frontOnly = new[]
        {
            new CardFieldDefinition(Guid.NewGuid(), "Prompt", CardFieldSide.Front, CardFieldType.Text, true, 1)
        };

        var result = CardTemplate.Create(Guid.NewGuid(), _academyId, "Invalid Template", "No back field", frontOnly);

        Assert.True(result.IsFailure);
        Assert.Contains("Back", result.Error);
    }

    [Fact]
    public void CardTemplate_Create_ShouldSucceed_WithFrontAndBackFields()
    {
        var fields = new[]
        {
            new CardFieldDefinition(Guid.NewGuid(), "Position", CardFieldSide.Front, CardFieldType.Text, true, 1),
            new CardFieldDefinition(Guid.NewGuid(), "Counter", CardFieldSide.Back, CardFieldType.MultilineText, true, 2),
            new CardFieldDefinition(Guid.NewGuid(), "Video", CardFieldSide.Back, CardFieldType.VideoUrl, false, 3)
        };

        var result = CardTemplate.Create(Guid.NewGuid(), _academyId, "BJJ Counter", "Positional reactions", fields);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Fields.Count);
    }

    [Fact]
    public void DeckSchedulingOptions_Create_ShouldValidateParameters()
    {
        var invalidSteps = DeckSchedulingOptions.Create(
            learningStepsMinutes: "0, -5",
            graduatingIntervalDays: 1,
            easyIntervalDays: 4,
            startingEaseFactor: 2.5,
            minimumEaseFactor: 1.3,
            easyBonusMultiplier: 1.3,
            hardIntervalMultiplier: 1.2,
            intervalModifier: 1.0,
            lapseIntervalMultiplier: 0.0,
            maximumIntervalDays: 365,
            dailyNewCardsLimit: 20,
            dailyReviewLimit: 100);

        Assert.True(invalidSteps.IsFailure);

        var validCustom = DeckSchedulingOptions.Create(
            learningStepsMinutes: "2, 15, 60",
            graduatingIntervalDays: 3,
            easyIntervalDays: 7,
            startingEaseFactor: 2.6,
            minimumEaseFactor: 1.4,
            easyBonusMultiplier: 1.4,
            hardIntervalMultiplier: 1.25,
            intervalModifier: 1.1,
            lapseIntervalMultiplier: 0.25,
            maximumIntervalDays: 180,
            dailyNewCardsLimit: 15,
            dailyReviewLimit: 80);

        Assert.True(validCustom.IsSuccess);
        Assert.Equal([2, 15, 60], validCustom.Value.GetLearningSteps());
    }

    [Fact]
    public void Sm2Scheduler_NewCard_ShouldProgressThroughLearningStepsToReview()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var options = DeckSchedulingOptions.Default();
        var deck = CreateSampleDeck(options);
        var card = AddSampleCard(deck, now);

        Assert.Equal(CardState.New, card.State);
        Assert.Equal(CardMaturityTier.New, card.MaturityTier);

        var firstGood = Sm2Scheduler.CalculateNextSchedule(card, options, ReviewRating.Good, now);
        Assert.Equal(CardState.Learning, firstGood.NextState);
        Assert.Equal(1, firstGood.NextStepIndex);
        Assert.Equal(now.AddMinutes(10), firstGood.NextDueAt);
        card.ApplySchedule(firstGood, now);
        Assert.Equal(CardMaturityTier.Learning, card.MaturityTier);

        var secondGood = Sm2Scheduler.CalculateNextSchedule(card, options, ReviewRating.Good, now.AddMinutes(10));
        Assert.Equal(CardState.Review, secondGood.NextState);
        Assert.Equal(1.0, secondGood.NextIntervalDays);
        Assert.Equal(1, secondGood.NextRepetitions);
        card.ApplySchedule(secondGood, now.AddMinutes(10));
        Assert.Equal(CardMaturityTier.Young, card.MaturityTier);
    }

    [Fact]
    public void Sm2Scheduler_ReviewCard_ShouldAdjustEaseAndMaturityAndHandleLapse()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var options = DeckSchedulingOptions.Create(
            learningStepsMinutes: "5, 20",
            graduatingIntervalDays: 1,
            easyIntervalDays: 4,
            startingEaseFactor: 2.5,
            minimumEaseFactor: 1.3,
            easyBonusMultiplier: 1.3,
            hardIntervalMultiplier: 1.2,
            intervalModifier: 1.0,
            lapseIntervalMultiplier: 0.2,
            maximumIntervalDays: 365,
            dailyNewCardsLimit: 20,
            dailyReviewLimit: 100).Value;

        var deck = CreateSampleDeck(options);
        var card = AddSampleCard(deck, now);

        card.RescheduleManual(10, now);
        Assert.Equal(CardState.Review, card.State);
        Assert.Equal(CardMaturityTier.Young, card.MaturityTier);

        var easyReview = Sm2Scheduler.CalculateNextSchedule(card, options, ReviewRating.Easy, now);
        Assert.Equal(CardState.Review, easyReview.NextState);
        Assert.True(easyReview.NextIntervalDays >= 21);
        Assert.Equal(2.65, easyReview.NextEaseFactor, 2);
        card.ApplySchedule(easyReview, now);
        Assert.Equal(CardMaturityTier.Mature, card.MaturityTier);

        var lapseReview = Sm2Scheduler.CalculateNextSchedule(card, options, ReviewRating.Again, now.AddDays(30));
        Assert.Equal(CardState.Relearning, lapseReview.NextState);
        Assert.Equal(1, lapseReview.NextLapses);
        Assert.Equal(2.45, lapseReview.NextEaseFactor, 2);
        Assert.Equal(Math.Round(easyReview.NextIntervalDays * 0.2, 1), lapseReview.NextIntervalDays);
    }

    [Fact]
    public void Sm2Scheduler_PreviewIntervals_ShouldReturnFourFormattedLabels()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var options = DeckSchedulingOptions.Default();
        var deck = CreateSampleDeck(options);
        var card = AddSampleCard(deck, now);

        var preview = Sm2Scheduler.PreviewIntervals(card, options, now);

        Assert.Equal("1m", preview.Again);
        Assert.Equal("6m", preview.Hard);
        Assert.Equal("10m", preview.Good);
        Assert.Equal("4d", preview.Easy);
    }

    [Fact]
    public void Flashcard_LifecycleOperations_SuspendResetAndManualReschedule_ShouldWork()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var deck = CreateSampleDeck(DeckSchedulingOptions.Default());
        var card = AddSampleCard(deck, now);

        card.RescheduleManual(25, now.AddDays(25));
        Assert.Equal(CardState.Review, card.State);
        Assert.Equal(CardMaturityTier.Mature, card.MaturityTier);

        card.Suspend();
        Assert.Equal(CardState.Suspended, card.State);
        Assert.Equal(CardMaturityTier.Suspended, card.MaturityTier);

        card.Unsuspend(now);
        Assert.Equal(CardState.Review, card.State);
        Assert.Equal(CardMaturityTier.Mature, card.MaturityTier);

        card.ResetProgress(2.5, now);
        Assert.Equal(CardState.New, card.State);
        Assert.Equal(0, card.Repetitions);
        Assert.Equal(0, card.Lapses);
        Assert.Equal(2.5, card.EaseFactor);
        Assert.Equal(CardMaturityTier.New, card.MaturityTier);
    }

    [Fact]
    public void StudyDeck_RecordReview_WithRescheduleFalse_ShouldLogReviewWithoutMutatingCardSchedule()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var deck = CreateSampleDeck(DeckSchedulingOptions.Default());
        var card = AddSampleCard(deck, now);

        var cramResult = deck.RecordReview(card.Id, ReviewRating.Easy, now, reschedule: false);

        Assert.True(cramResult.IsSuccess);
        Assert.Equal(CardState.New, card.State);
        Assert.Equal(0, card.Repetitions);
        Assert.Equal(ReviewRating.Easy, cramResult.Value.Rating);
    }

    [Fact]
    public void SpacedRepetitionPresets_ShouldProvideValidTemplatesAndDecks()
    {
        var templates = SpacedRepetitionPresets.CreateDefaultTemplates(_academyId);
        var decks = SpacedRepetitionPresets.CreateStarterDecks(_academyId, templates);

        Assert.Equal(3, templates.Count);
        Assert.Equal(2, decks.Count);
        Assert.All(decks, d => Assert.True(d.Cards.Count >= 4));
    }

    private StudyDeck CreateSampleDeck(DeckSchedulingOptions options)
    {
        var templateId = Guid.NewGuid();
        return StudyDeck.Create(
            Guid.NewGuid(),
            _academyId,
            templateId,
            "Guard Retention",
            "Open guard counters",
            "Guard",
            options).Value;
    }

    private static Flashcard AddSampleCard(StudyDeck deck, DateTime now)
    {
        var cardId = Guid.NewGuid();
        var fields = new[]
        {
            new CardFieldValue(Guid.NewGuid(), Guid.NewGuid(), "Front Prompt", CardFieldSide.Front, CardFieldType.Text, "Knee Slice Defense", 1),
            new CardFieldValue(Guid.NewGuid(), Guid.NewGuid(), "Back Answer", CardFieldSide.Back, CardFieldType.MultilineText, "Frame neck, hip escape, recover knee shield", 2)
        };
        deck.AddCard(cardId, deck.TemplateId, ["guard", "retention"], fields, now);
        return deck.Cards.First(c => c.Id == cardId);
    }
}
