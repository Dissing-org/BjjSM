using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Infrastructure.Pdf;
using BjjSM.Modules.Curriculum.Infrastructure.Persistence;
using BjjSM.Modules.Memberships.Application;
using BjjSM.Modules.Memberships.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);

var membershipsConn = builder.Configuration.GetConnectionString("MembershipsDb") 
    ?? $"Data Source={Path.Combine(dataDir, "memberships.db")}";

var curriculumConn = builder.Configuration.GetConnectionString("CurriculumDb") 
    ?? $"Data Source={Path.Combine(dataDir, "curriculum.db")}";

builder.Services.AddDbContext<MembershipsDbContext>(opt => opt.UseSqlite(membershipsConn));
builder.Services.AddDbContext<CurriculumDbContext>(opt => opt.UseSqlite(curriculumConn));

builder.Services.AddScoped<IPractitionerRepository, PractitionerRepository>();
builder.Services.AddScoped<IMonthlyPlanRepository, MonthlyPlanRepository>();
builder.Services.AddScoped<ISyllabusPdfGenerator, QuestPdfSyllabusGenerator>();
builder.Services.AddScoped<IAnnualWheelRepository, AnnualWheelRepository>();
builder.Services.AddScoped<ICurriculumThemeRepository, CurriculumThemeRepository>();
builder.Services.AddScoped<IAnnualWheelService, AnnualWheelService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseFastEndpoints();

using (var scope = app.Services.CreateScope())
{
    var memDb = scope.ServiceProvider.GetRequiredService<MembershipsDbContext>();
    memDb.Database.EnsureCreated();
    var memSql = memDb.Database.GenerateCreateScript()
        .Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ")
        .Replace("CREATE INDEX ", "CREATE INDEX IF NOT EXISTS ")
        .Replace("CREATE UNIQUE INDEX ", "CREATE UNIQUE INDEX IF NOT EXISTS ");
    memDb.Database.ExecuteSqlRaw(memSql);

    if (!memDb.Practitioners.Any())
    {
        var coachId = Guid.NewGuid();
        var academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var p1 = BjjSM.Modules.Memberships.Domain.Practitioner.Create(
            academyId, "Lucas", "Leite", "lucas.leite@bjj.academy", 
            BjjSM.Modules.Memberships.Domain.PractitionerRole.Instructor, 
            BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Black, 3).Value).Value;
        p1.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Black, 1).Value, DateTime.UtcNow.AddYears(-6), coachId, "First degree awarded after 3 active years.");
        p1.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Black, 2).Value, DateTime.UtcNow.AddYears(-3), coachId, "Second degree black belt.");
        p1.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Black, 3).Value, DateTime.UtcNow.AddMonths(-4), coachId, "Third degree black belt professor.");

        var p2 = BjjSM.Modules.Memberships.Domain.Practitioner.Create(
            academyId, "Marcus", "Almeida", "buchecha@bjj.academy", 
            BjjSM.Modules.Memberships.Domain.PractitionerRole.Student, 
            BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Brown, 2).Value).Value;
        p2.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Purple, 4).Value, DateTime.UtcNow.AddMonths(-14), coachId, "State championship gold medalist.");
        p2.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Brown, 0).Value, DateTime.UtcNow.AddMonths(-8), coachId, "Promoted to Brown belt.");
        p2.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Brown, 2).Value, DateTime.UtcNow.AddMonths(-2), coachId, "Submission victory in superfight.");

        var p3 = BjjSM.Modules.Memberships.Domain.Practitioner.Create(
            academyId, "Bianca", "Basilio", "bianca.basilio@bjj.academy", 
            BjjSM.Modules.Memberships.Domain.PractitionerRole.Student, 
            BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Purple, 3).Value).Value;
        p3.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Blue, 4).Value, DateTime.UtcNow.AddMonths(-18), coachId, "Guard retention excellence.");
        p3.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Purple, 0).Value, DateTime.UtcNow.AddMonths(-12), coachId, "Promoted to Purple belt.");
        p3.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateAdult(BjjSM.Modules.Memberships.Domain.AdultBelt.Purple, 3).Value, DateTime.UtcNow.AddMonths(-3), coachId, "Footlock mastery and tournament run.");

        var p4 = BjjSM.Modules.Memberships.Domain.Practitioner.Create(
            academyId, "Mateo", "Silva", "mateo.kids@bjj.academy", 
            BjjSM.Modules.Memberships.Domain.PractitionerRole.Student, 
            BjjSM.Modules.Memberships.Domain.Rank.CreateKids(BjjSM.Modules.Memberships.Domain.KidsBelt.Yellow, 2).Value).Value;
        p4.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateKids(BjjSM.Modules.Memberships.Domain.KidsBelt.Grey, 4).Value, DateTime.UtcNow.AddMonths(-10), coachId, "Kids fundamentals syllabus complete.");
        p4.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateKids(BjjSM.Modules.Memberships.Domain.KidsBelt.Yellow, 0).Value, DateTime.UtcNow.AddMonths(-5), coachId, "Graduated to Yellow belt.");
        p4.Promote(BjjSM.Modules.Memberships.Domain.Rank.CreateKids(BjjSM.Modules.Memberships.Domain.KidsBelt.Yellow, 2).Value, DateTime.UtcNow.AddMonths(-1), coachId, "Respect, discipline and takedown technique.");

        memDb.Practitioners.AddRange(p1, p2, p3, p4);
        memDb.SaveChanges();
    }

    var curDb = scope.ServiceProvider.GetRequiredService<CurriculumDbContext>();
    curDb.Database.EnsureCreated();
    var curSql = curDb.Database.GenerateCreateScript()
        .Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ")
        .Replace("CREATE INDEX ", "CREATE INDEX IF NOT EXISTS ")
        .Replace("CREATE UNIQUE INDEX ", "CREATE UNIQUE INDEX IF NOT EXISTS ");
    curDb.Database.ExecuteSqlRaw(curSql);

    if (!curDb.CurriculumThemes.Any())
    {
        curDb.CurriculumThemes.AddRange(BjjSM.Modules.Curriculum.Domain.CurriculumThemePresets.CreatePresets());
        curDb.SaveChanges();
    }

    if (!curDb.MonthlyPlans.Any())
    {
        var academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var programId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var now = DateTime.UtcNow;
        var plan = BjjSM.Modules.Curriculum.Domain.MonthlyPlan.Create(
            academyId,
            programId,
            now.Year,
            now.Month,
            "Guard Retention, Armbar Submissions & Pin Escapes",
            Guid.NewGuid(),
            4).Value;

        plan.AddTechnique(1, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Hip Escape (Shrimping) & Granby Roll Drill", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Warmup, "Maintain tight frame against imaginary chest pressure", null, 1));
        plan.AddTechnique(1, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Cross-Collar Choke & Armbar Combination", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Deep thumb grip, isolate tricep, pivot 90 degrees", "https://youtube.com", 2));
        plan.AddTechnique(1, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Closed Guard King of the Mat (Pass or Submit)", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.SituationalDrill, "Bottom player must sweep or submit, top player must pass", null, 3));

        plan.AddTechnique(2, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Scissor Sweep & Knee Shield Transition", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Kick bottom leg horizontally, chop with top shin across abdomen", null, 1));
        plan.AddTechnique(2, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Triangle Choke from Failed Scissor Sweep", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Shoot hips high, lock figure-4 on opponent's neck", null, 2));

        plan.AddTechnique(3, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Knee Slice Guard Pass", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Underhook opposite arm, pin far thigh, drive knee across shin line", null, 1));
        plan.AddTechnique(3, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Long Step Counter against Reverse De La Riva", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Disengage hook, whip trailing leg back, secure collar frame", null, 2));

        plan.AddTechnique(4, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "Side Control Frame Escape to Reguard", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Bridge hard into partner to create space, insert near knee", null, 1));
        plan.AddTechnique(4, new BjjSM.Modules.Curriculum.Domain.TechniqueDrill(Guid.NewGuid(), "North-South Spin & Turtle Recovery", BjjSM.Modules.Curriculum.Domain.TechniqueCategory.Technique, "Protect chin, tuck elbows, rotate on top of shoulders", null, 2));

        curDb.MonthlyPlans.Add(plan);
        curDb.SaveChanges();
    }
}

app.Run();
