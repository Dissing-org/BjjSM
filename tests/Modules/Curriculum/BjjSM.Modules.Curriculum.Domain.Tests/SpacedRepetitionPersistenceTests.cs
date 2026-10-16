using BjjSM.Modules.Curriculum.Domain;
using BjjSM.Modules.Curriculum.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class SpacedRepetitionPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CurriculumDbContext> _options;
    private readonly Guid _academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public SpacedRepetitionPersistenceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<CurriculumDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new CurriculumDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SaveAndReload_TemplateDeckCardsAndReviewLogs_ShouldRoundTripSuccessfully()
    {
        var templateId = Guid.NewGuid();
        var fieldFrontId = Guid.NewGuid();
        var fieldBackId = Guid.NewGuid();

        var template = CardTemplate.Create(
            templateId,
            _academyId,
            "Custom BJJ Sequence",
            "Positional cue and finish",
            [
                new CardFieldDefinition(fieldFrontId, "Position Cue", CardFieldSide.Front, CardFieldType.Text, true, 1),
                new CardFieldDefinition(fieldBackId, "Technique Execution", CardFieldSide.Back, CardFieldType.MultilineText, true, 2)
            ]).Value;

        var options = DeckSchedulingOptions.Create(
            learningStepsMinutes: "3, 12",
            graduatingIntervalDays: 2,
            easyIntervalDays: 5,
            startingEaseFactor: 2.5,
            minimumEaseFactor: 1.3,
            easyBonusMultiplier: 1.3,
            hardIntervalMultiplier: 1.2,
            intervalModifier: 1.0,
            lapseIntervalMultiplier: 0.2,
            maximumIntervalDays: 365,
            dailyNewCardsLimit: 25,
            dailyReviewLimit: 120).Value;

        var deckId = Guid.NewGuid();
        var deck = StudyDeck.Create(
            deckId,
            _academyId,
            templateId,
            "Leg Lock Entries",
            "Saddle and Outside Ashi transitions",
            "No-Gi Leg Locks",
            options).Value;

        var cardId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 16, 10, 0, 0, DateTimeKind.Utc);
        deck.AddCard(
            cardId,
            templateId,
            ["ashi", "heel-hook"],
            [
                new CardFieldValue(Guid.NewGuid(), fieldFrontId, "Position Cue", CardFieldSide.Front, CardFieldType.Text, "Reverse X-Guard", 1),
                new CardFieldValue(Guid.NewGuid(), fieldBackId, "Technique Execution", CardFieldSide.Back, CardFieldType.MultilineText, "Elevate far leg and shoot into Saddle", 2)
            ],
            now);

        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new SpacedRepetitionRepository(context);
            await repo.AddTemplateAsync(template);
            await repo.AddDeckAsync(deck);
        }

        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new SpacedRepetitionRepository(context);
            var loadedDeck = await repo.GetDeckByIdAsync(deckId);
            Assert.NotNull(loadedDeck);
            Assert.Equal("3, 12", loadedDeck.SchedulingOptions.LearningStepsMinutes);
            Assert.Equal(2, loadedDeck.SchedulingOptions.GraduatingIntervalDays);
            Assert.Single(loadedDeck.Cards);

            var card = loadedDeck.Cards.First();
            Assert.Equal(2, card.FieldValues.Count);
            Assert.Equal(["ashi", "heel-hook"], card.GetTags());

            var reviewResult = loadedDeck.RecordReview(card.Id, ReviewRating.Easy, now, reschedule: true);
            Assert.True(reviewResult.IsSuccess);

            loadedDeck.UpdateCard(
                card.Id,
                ["ashi", "heel-hook", "saddle"],
                [
                    new CardFieldValue(Guid.NewGuid(), fieldFrontId, "Position Cue", CardFieldSide.Front, CardFieldType.Text, "Reverse X-Guard Updated", 1),
                    new CardFieldValue(Guid.NewGuid(), fieldBackId, "Technique Execution", CardFieldSide.Back, CardFieldType.MultilineText, "Elevate far leg and lock figure-four", 2)
                ]);

            await repo.UpdateDeckAsync(loadedDeck);
            await repo.AddReviewLogAsync(reviewResult.Value);
        }

        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new SpacedRepetitionRepository(context);
            var reloadedDeck = await repo.GetDeckByIdAsync(deckId);
            Assert.NotNull(reloadedDeck);

            var card = reloadedDeck.Cards.First();
            Assert.Equal(CardState.Review, card.State);
            Assert.Equal(5.0, card.IntervalDays);
            Assert.Contains("saddle", card.GetTags());
            Assert.Contains(card.FieldValues, f => f.Value == "Reverse X-Guard Updated");

            var logs = await repo.GetReviewLogsAsync(_academyId);
            Assert.Single(logs);
            Assert.Equal(ReviewRating.Easy, logs[0].Rating);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
