using BjjSM.Modules.Memberships.Domain;
using BjjSM.Modules.Memberships.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BjjSM.Modules.Memberships.Domain.Tests;

public class MembershipsPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<MembershipsDbContext> _options;

    public MembershipsPersistenceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<MembershipsDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new MembershipsDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SaveAndReload_ShouldPreservePractitionerAggregate_WithRankAndGraduationTimeline()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var initialWhite = Rank.CreateAdult(AdultBelt.White, 0).Value;

        var practitioner = Practitioner.Create(
            academyId,
            "Bernardo",
            "Faria",
            "bernardo@bjjfanatics.com",
            PractitionerRole.Student | PractitionerRole.Instructor,
            initialWhite).Value;

        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 2).Value;
        practitioner.Promote(blueRank, DateTime.UtcNow.AddMonths(-12), coachId, "Deep half guard success");

        var purpleRank = Rank.CreateAdult(AdultBelt.Purple, 0).Value;
        practitioner.Promote(purpleRank, DateTime.UtcNow.AddMonths(-6), coachId, "World championship gold");

        // Act 1: Persist in unit of work 1
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            await repo.AddAsync(practitioner);
            await repo.SaveChangesAsync();
        }

        // Act 2: Reload in fresh unit of work 2
        Practitioner? loaded;
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            loaded = await repo.GetByIdAsync(practitioner.Id);
        }

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Bernardo", loaded.FirstName);
        Assert.Equal("Faria", loaded.LastName);
        Assert.Equal(PractitionerRole.Student | PractitionerRole.Instructor, loaded.Roles);
        Assert.Equal(purpleRank, loaded.CurrentRank);
        Assert.Equal(2, loaded.Graduations.Count);

        var firstGrad = loaded.Graduations.OrderBy(g => g.AwardedAt).First();
        Assert.Equal(blueRank, firstGrad.Rank);
        Assert.Equal("Deep half guard success", firstGrad.Notes);
    }

    [Fact]
    public async Task CreateMember_WithZeroGraduations_PersistsAndReloadsSuccessfully()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var initialRank = Rank.CreateAdult(AdultBelt.White, 0).Value;

        var practitioner = Practitioner.Create(
            academyId,
            "Helio",
            "Gracie",
            "helio@gracie.com",
            PractitionerRole.Instructor,
            initialRank).Value;

        // Act 1: Persist in unit of work 1
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            await repo.AddAsync(practitioner);
            await repo.SaveChangesAsync();
        }

        // Act 2: Reload in fresh unit of work 2
        Practitioner? loaded;
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            loaded = await repo.GetByIdAsync(practitioner.Id);
        }

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Helio", loaded.FirstName);
        Assert.Equal("Gracie", loaded.LastName);
        Assert.Equal(initialRank, loaded.InitialRank);
        Assert.Equal(initialRank, loaded.CurrentRank);
        Assert.Equal(loaded.InitialRank, loaded.CurrentRank);
        Assert.Empty(loaded.Graduations);
    }

    [Fact]
    public async Task PromoteMember_OnTrackedAggregate_PersistsNewGraduationWithoutConcurrencyException()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;

        var practitioner = Practitioner.Create(
            academyId,
            "Royce",
            "Gracie",
            "royce@gracie.com",
            PractitionerRole.Student,
            whiteRank).Value;

        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            await repo.AddAsync(practitioner);
            await repo.SaveChangesAsync();
        }

        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 1).Value;
        var promotionDate = DateTime.UtcNow.AddMonths(-1);

        // Act: Load tracked aggregate in context 2, promote, save
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            var tracked = await repo.GetByIdAsync(practitioner.Id);
            Assert.NotNull(tracked);

            var promoteResult = tracked.Promote(blueRank, promotionDate, coachId, "First blue stripe");
            Assert.True(promoteResult.IsSuccess);

            await repo.SaveChangesAsync();
        }

        // Assert: Reload in context 3
        Practitioner? reloaded;
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            reloaded = await repo.GetByIdAsync(practitioner.Id);
        }

        Assert.NotNull(reloaded);
        Assert.Equal(blueRank, reloaded.CurrentRank);
        Assert.Equal(whiteRank, reloaded.InitialRank);
        Assert.Single(reloaded.Graduations);

        var grad = reloaded.Graduations.First();
        Assert.Equal(blueRank, grad.Rank);
        Assert.Equal("First blue stripe", grad.Notes);
        Assert.Equal(coachId, grad.AwardedBy);
    }

    [Fact]
    public async Task CorrectGraduation_OnTrackedAggregate_PersistsSuccessfully()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;

        var practitioner = Practitioner.Create(
            academyId,
            "Rickson",
            "Gracie",
            "rickson@gracie.com",
            PractitionerRole.Instructor,
            whiteRank).Value;

        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 1).Value;
        practitioner.Promote(blueRank, DateTime.UtcNow.AddMonths(-6), coachId, "Initial promotion");

        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            await repo.AddAsync(practitioner);
            await repo.SaveChangesAsync();
        }

        var gradId = practitioner.Graduations.First().Id;
        var correctedRank = Rank.CreateAdult(AdultBelt.Blue, 2).Value;
        var correctedDate = DateTime.UtcNow.AddMonths(-5);

        // Act: Load tracked aggregate in context 2, correct graduation, save
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            var tracked = await repo.GetByIdAsync(practitioner.Id);
            Assert.NotNull(tracked);

            var correctResult = tracked.CorrectGraduation(gradId, correctedRank, correctedDate, coachId, "Corrected to 2 stripes");
            Assert.True(correctResult.IsSuccess);

            await repo.SaveChangesAsync();
        }

        // Assert: Reload in context 3
        Practitioner? reloaded;
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            reloaded = await repo.GetByIdAsync(practitioner.Id);
        }

        Assert.NotNull(reloaded);
        Assert.Equal(correctedRank, reloaded.CurrentRank);
        Assert.Single(reloaded.Graduations);

        var grad = reloaded.Graduations.First();
        Assert.Equal(correctedRank, grad.Rank);
        Assert.Equal("Corrected to 2 stripes", grad.Notes);
    }

    [Fact]
    public async Task RemoveGraduation_RemovingLastGraduation_RevertsToInitialRankAndDeletesRow()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var whiteRank = Rank.CreateAdult(AdultBelt.White, 0).Value;

        var practitioner = Practitioner.Create(
            academyId,
            "Roger",
            "Gracie",
            "roger@gracie.com",
            PractitionerRole.Student,
            whiteRank).Value;

        var blueRank = Rank.CreateAdult(AdultBelt.Blue, 0).Value;
        practitioner.Promote(blueRank, DateTime.UtcNow.AddMonths(-3), coachId, "Accidental promotion");

        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            await repo.AddAsync(practitioner);
            await repo.SaveChangesAsync();
        }

        var gradId = practitioner.Graduations.First().Id;

        // Act: Load tracked aggregate in context 2, remove graduation, save
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            var tracked = await repo.GetByIdAsync(practitioner.Id);
            Assert.NotNull(tracked);

            var removeResult = tracked.RemoveGraduation(gradId);
            Assert.True(removeResult.IsSuccess);

            await repo.SaveChangesAsync();
        }

        // Assert: Reload in context 3
        Practitioner? reloaded;
        await using (var context = new MembershipsDbContext(_options))
        {
            var repo = new PractitionerRepository(context);
            reloaded = await repo.GetByIdAsync(practitioner.Id);

            var totalGraduationsCount = await context.Graduations.CountAsync();
            Assert.Equal(0, totalGraduationsCount);
        }

        Assert.NotNull(reloaded);
        Assert.Equal(whiteRank, reloaded.CurrentRank);
        Assert.Equal(whiteRank, reloaded.InitialRank);
        Assert.Empty(reloaded.Graduations);
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
