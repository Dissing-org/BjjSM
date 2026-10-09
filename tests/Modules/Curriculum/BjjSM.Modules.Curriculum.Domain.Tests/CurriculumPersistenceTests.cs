using BjjSM.Modules.Curriculum.Domain;
using BjjSM.Modules.Curriculum.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class CurriculumPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CurriculumDbContext> _options;

    public CurriculumPersistenceTests()
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
    public async Task SaveAndReload_ShouldPreserveMonthlyPlan_WithWeeksAndTechniques()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var plan = MonthlyPlan.Create(
            academyId,
            programId,
            year: 2026,
            month: 11,
            objective: "Back Control, Seatbelt Grip, and Choke Finishing",
            createdByInstructorId: coachId,
            weekCount: 4).Value;

        plan.UpdateWeekTheme(1, "Taking the Back from Turtle");
        plan.UpdateWeekTheme(2, "Arm Drag to Back Control");
        plan.UpdateWeekTheme(3, "Body Triangle & Position Maintenance");
        plan.UpdateWeekTheme(4, "Rear Naked Choke & Collar Strangle");
        plan.AddTechnique(1, new TechniqueDrill(
            Guid.NewGuid(),
            "Chair Sit to Back Take",
            TechniqueCategory.Technique,
            "Keep chest glued to opponent's back",
            "https://example.com/chair-sit",
            SortOrder: 1));

        plan.Publish(coachId);

        // Act 1: Persist in unit of work 1
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            await repo.AddAsync(plan);
            await repo.SaveChangesAsync();
        }

        // Act 2: Reload in fresh unit of work 2
        MonthlyPlan? loaded;
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            loaded = await repo.GetByIdAsync(plan.Id);
        }

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(2026, loaded.Year);
        Assert.Equal(11, loaded.Month);
        Assert.Equal(PlanStatus.Published, loaded.Status);
        Assert.Equal("Back Control, Seatbelt Grip, and Choke Finishing", loaded.MonthlyObjective);
        Assert.Equal(4, loaded.Weeks.Count);

        var week1 = loaded.Weeks.First(w => w.WeekNumber == 1);
        Assert.Equal("Taking the Back from Turtle", week1.Theme);
        Assert.Single(week1.Techniques);

        var tech = week1.Techniques.First();
        Assert.Equal("Chair Sit to Back Take", tech.Title);
        Assert.Equal(TechniqueCategory.Technique, tech.Category);
        Assert.Equal("https://example.com/chair-sit", tech.ReferenceUrl);
    }

    [Fact]
    public async Task AddAndUpdateAndRemoveTechnique_OnExistingPlan_ShouldPersistSuccessfully()
    {
        var academyId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var plan = MonthlyPlan.Create(academyId, programId, 2026, 12, "December Plan", coachId, 4).Value;
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            await repo.AddAsync(plan);
            await repo.SaveChangesAsync();
        }

        // Add technique
        var techId = Guid.NewGuid();
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            var loaded = await repo.GetByIdAsync(plan.Id);
            Assert.NotNull(loaded);
            loaded.AddTechnique(1, new TechniqueDrill(techId, "Armbar", TechniqueCategory.Technique, "Notes", null, SortOrder: 1));
            await repo.SaveChangesAsync();
        }

        // Verify added & update technique
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            var loaded = await repo.GetByIdAsync(plan.Id);
            Assert.NotNull(loaded);
            var tech = loaded.Weeks.First(w => w.WeekNumber == 1).Techniques.First();
            Assert.Equal("Armbar", tech.Title);

            loaded.UpdateTechnique(1, techId, "Armbar Updated", TechniqueCategory.Technique, "New notes", null, 1);
            await repo.SaveChangesAsync();
        }

        // Verify updated & delete technique
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            var loaded = await repo.GetByIdAsync(plan.Id);
            Assert.NotNull(loaded);
            var tech = loaded.Weeks.First(w => w.WeekNumber == 1).Techniques.First();
            Assert.Equal("Armbar Updated", tech.Title);

            loaded.RemoveTechnique(1, techId);
            await repo.SaveChangesAsync();
        }

        // Verify deleted
        await using (var context = new CurriculumDbContext(_options))
        {
            var repo = new MonthlyPlanRepository(context);
            var loaded = await repo.GetByIdAsync(plan.Id);
            Assert.NotNull(loaded);
            Assert.Empty(loaded.Weeks.First(w => w.WeekNumber == 1).Techniques);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
