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

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
