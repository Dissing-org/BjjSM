using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public class CurriculumDbContext : DbContext
{
    public DbSet<MonthlyPlan> MonthlyPlans => Set<MonthlyPlan>();
    public DbSet<WeeklyFocus> WeeklyFocuses => Set<WeeklyFocus>();
    public DbSet<TechniqueDrill> TechniqueDrills => Set<TechniqueDrill>();

    public DbSet<AnnualWheel> AnnualWheels => Set<AnnualWheel>();
    public DbSet<AnnualWheelSlot> AnnualWheelSlots => Set<AnnualWheelSlot>();
    public DbSet<CurriculumTheme> CurriculumThemes => Set<CurriculumTheme>();
    public DbSet<ThemeDrillTemplate> ThemeDrillTemplates => Set<ThemeDrillTemplate>();

    public DbSet<CardTemplate> CardTemplates => Set<CardTemplate>();
    public DbSet<CardFieldDefinition> CardFieldDefinitions => Set<CardFieldDefinition>();
    public DbSet<StudyDeck> StudyDecks => Set<StudyDeck>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<CardFieldValue> CardFieldValues => Set<CardFieldValue>();
    public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();

    public CurriculumDbContext(DbContextOptions<CurriculumDbContext> options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<TechniqueDrill>())
        {
            if (entry.State == EntityState.Modified)
            {
                var fkProp = entry.Property<Guid>("WeeklyFocusId");
                if (fkProp.OriginalValue == Guid.Empty)
                {
                    entry.State = EntityState.Added;
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<CardFieldDefinition>())
        {
            if (entry.State == EntityState.Modified)
            {
                var fkProp = entry.Property<Guid>("CardTemplateId");
                if (fkProp.OriginalValue == Guid.Empty)
                {
                    entry.State = EntityState.Added;
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<CardFieldValue>())
        {
            if (entry.State == EntityState.Modified)
            {
                var fkProp = entry.Property<Guid>("FlashcardId");
                if (fkProp.OriginalValue == Guid.Empty)
                {
                    entry.State = EntityState.Added;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MonthlyPlanConfiguration());
        modelBuilder.ApplyConfiguration(new WeeklyFocusConfiguration());
        modelBuilder.ApplyConfiguration(new TechniqueDrillConfiguration());

        modelBuilder.ApplyConfiguration(new AnnualWheelConfiguration());
        modelBuilder.ApplyConfiguration(new AnnualWheelSlotConfiguration());
        modelBuilder.ApplyConfiguration(new CurriculumThemeConfiguration());
        modelBuilder.ApplyConfiguration(new ThemeDrillTemplateConfiguration());

        modelBuilder.ApplyConfiguration(new CardTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new CardFieldDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new StudyDeckConfiguration());
        modelBuilder.ApplyConfiguration(new FlashcardConfiguration());
        modelBuilder.ApplyConfiguration(new CardFieldValueConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewLogConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

public class MonthlyPlanConfiguration : IEntityTypeConfiguration<MonthlyPlan>
{
    public void Configure(EntityTypeBuilder<MonthlyPlan> builder)
    {
        builder.ToTable("MonthlyPlans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.AcademyId).IsRequired();
        builder.Property(p => p.ProgramId).IsRequired();
        builder.Property(p => p.Year).IsRequired();
        builder.Property(p => p.Month).IsRequired();
        builder.Property(p => p.MonthlyObjective).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.CreatedByInstructorId).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.LastModifiedAt).IsRequired();

        builder.HasIndex(p => new { p.AcademyId, p.ProgramId, p.Year, p.Month }).IsUnique();

        builder.HasMany(p => p.Weeks)
            .WithOne()
            .HasForeignKey("MonthlyPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Weeks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class WeeklyFocusConfiguration : IEntityTypeConfiguration<WeeklyFocus>
{
    public void Configure(EntityTypeBuilder<WeeklyFocus> builder)
    {
        builder.ToTable("WeeklyFocuses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.WeekNumber).IsRequired();
        builder.Property(w => w.Theme).HasMaxLength(255).IsRequired();

        builder.HasMany(w => w.Techniques)
            .WithOne()
            .HasForeignKey("WeeklyFocusId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Techniques)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class TechniqueDrillConfiguration : IEntityTypeConfiguration<TechniqueDrill>
{
    public void Configure(EntityTypeBuilder<TechniqueDrill> builder)
    {
        builder.ToTable("TechniqueDrills");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property<Guid>("WeeklyFocusId").IsRequired();

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Category).IsRequired();
        builder.Property(t => t.CoachingNotes).HasMaxLength(1000);
        builder.Property(t => t.ReferenceUrl).HasMaxLength(500);
        builder.Property(t => t.SortOrder).IsRequired();
    }
}

public class AnnualWheelConfiguration : IEntityTypeConfiguration<AnnualWheel>
{
    public void Configure(EntityTypeBuilder<AnnualWheel> builder)
    {
        builder.ToTable("AnnualWheels");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.AcademyId).IsRequired();
        builder.Property(w => w.ProgramId).IsRequired();
        builder.Property(w => w.Year).IsRequired();
        builder.Property(w => w.Status).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.LastModifiedAt).IsRequired();

        builder.HasIndex(w => new { w.AcademyId, w.ProgramId, w.Year }).IsUnique();

        builder.HasMany(w => w.Slots)
            .WithOne()
            .HasForeignKey("AnnualWheelId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Slots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class AnnualWheelSlotConfiguration : IEntityTypeConfiguration<AnnualWheelSlot>
{
    public void Configure(EntityTypeBuilder<AnnualWheelSlot> builder)
    {
        builder.ToTable("AnnualWheelSlots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property<Guid>("AnnualWheelId").IsRequired();

        builder.Property(s => s.Month).IsRequired();
        builder.Property(s => s.ThemeId).IsRequired();
        builder.Property(s => s.ThemeName).HasMaxLength(255).IsRequired();
        builder.Property(s => s.Objective).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.IsInstantiated).IsRequired();
        builder.Property(s => s.MonthlyPlanId);

        builder.HasIndex("AnnualWheelId", "Month").IsUnique();
    }
}

public class CurriculumThemeConfiguration : IEntityTypeConfiguration<CurriculumTheme>
{
    public void Configure(EntityTypeBuilder<CurriculumTheme> builder)
    {
        builder.ToTable("CurriculumThemes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(255).IsRequired();
        builder.Property(t => t.Category).HasMaxLength(100).IsRequired();
        builder.Property(t => t.DefaultObjective).HasMaxLength(1000).IsRequired();

        builder.HasMany(t => t.DefaultDrills)
            .WithOne()
            .HasForeignKey("CurriculumThemeId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.DefaultDrills)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ThemeDrillTemplateConfiguration : IEntityTypeConfiguration<ThemeDrillTemplate>
{
    public void Configure(EntityTypeBuilder<ThemeDrillTemplate> builder)
    {
        builder.ToTable("ThemeDrillTemplates");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property<Guid>("CurriculumThemeId").IsRequired();

        builder.Property(d => d.Title).HasMaxLength(255).IsRequired();
        builder.Property(d => d.Category).IsRequired();
        builder.Property(d => d.CoachingNotes).HasMaxLength(1000);
        builder.Property(d => d.ReferenceUrl).HasMaxLength(500);
        builder.Property(d => d.TargetWeekNumber).IsRequired();
        builder.Property(d => d.SortOrder).IsRequired();
    }
}

public class CardTemplateConfiguration : IEntityTypeConfiguration<CardTemplate>
{
    public void Configure(EntityTypeBuilder<CardTemplate> builder)
    {
        builder.ToTable("CardTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.AcademyId).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.LastModifiedAt).IsRequired();

        builder.HasMany(t => t.Fields)
            .WithOne()
            .HasForeignKey("CardTemplateId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Fields)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class CardFieldDefinitionConfiguration : IEntityTypeConfiguration<CardFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CardFieldDefinition> builder)
    {
        builder.ToTable("CardFieldDefinitions");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();
        builder.Property<Guid>("CardTemplateId").IsRequired();

        builder.Property(f => f.Name).HasMaxLength(150).IsRequired();
        builder.Property(f => f.Side).IsRequired();
        builder.Property(f => f.FieldType).IsRequired();
        builder.Property(f => f.IsRequired).IsRequired();
        builder.Property(f => f.SortOrder).IsRequired();
    }
}

public class StudyDeckConfiguration : IEntityTypeConfiguration<StudyDeck>
{
    public void Configure(EntityTypeBuilder<StudyDeck> builder)
    {
        builder.ToTable("StudyDecks");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.AcademyId).IsRequired();
        builder.Property(d => d.TemplateId).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.Category).HasMaxLength(100).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.LastModifiedAt).IsRequired();

        builder.OwnsOne(d => d.SchedulingOptions, opt =>
        {
            opt.Property(o => o.LearningStepsMinutes).HasMaxLength(100).IsRequired();
            opt.Property(o => o.GraduatingIntervalDays).IsRequired();
            opt.Property(o => o.EasyIntervalDays).IsRequired();
            opt.Property(o => o.StartingEaseFactor).IsRequired();
            opt.Property(o => o.MinimumEaseFactor).IsRequired();
            opt.Property(o => o.EasyBonusMultiplier).IsRequired();
            opt.Property(o => o.HardIntervalMultiplier).IsRequired();
            opt.Property(o => o.IntervalModifier).IsRequired();
            opt.Property(o => o.LapseIntervalMultiplier).IsRequired();
            opt.Property(o => o.MaximumIntervalDays).IsRequired();
            opt.Property(o => o.DailyNewCardsLimit).IsRequired();
            opt.Property(o => o.DailyReviewLimit).IsRequired();
        });

        builder.HasMany(d => d.Cards)
            .WithOne()
            .HasForeignKey(c => c.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Cards)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("Flashcards");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.DeckId).IsRequired();
        builder.Property(c => c.TemplateId).IsRequired();
        builder.Property(c => c.State).IsRequired();
        builder.Property(c => c.LearningStepIndex).IsRequired();
        builder.Property(c => c.IntervalDays).IsRequired();
        builder.Property(c => c.EaseFactor).IsRequired();
        builder.Property(c => c.Repetitions).IsRequired();
        builder.Property(c => c.Lapses).IsRequired();
        builder.Property(c => c.DueAt).IsRequired();
        builder.Property(c => c.LastReviewedAt);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.TagsRaw).HasMaxLength(500);

        builder.Ignore(c => c.MaturityTier);

        builder.HasMany(c => c.FieldValues)
            .WithOne()
            .HasForeignKey("FlashcardId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.FieldValues)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class CardFieldValueConfiguration : IEntityTypeConfiguration<CardFieldValue>
{
    public void Configure(EntityTypeBuilder<CardFieldValue> builder)
    {
        builder.ToTable("CardFieldValues");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.Property<Guid>("FlashcardId").IsRequired();

        builder.Property(v => v.FieldDefinitionId).IsRequired();
        builder.Property(v => v.FieldName).HasMaxLength(150).IsRequired();
        builder.Property(v => v.Side).IsRequired();
        builder.Property(v => v.FieldType).IsRequired();
        builder.Property(v => v.Value).HasMaxLength(4000).IsRequired();
        builder.Property(v => v.SortOrder).IsRequired();
    }
}

public class ReviewLogConfiguration : IEntityTypeConfiguration<ReviewLog>
{
    public void Configure(EntityTypeBuilder<ReviewLog> builder)
    {
        builder.ToTable("ReviewLogs");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.DeckId).IsRequired();
        builder.Property(r => r.CardId).IsRequired();
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.PreviousState).IsRequired();
        builder.Property(r => r.NewState).IsRequired();
        builder.Property(r => r.IntervalDaysAfter).IsRequired();
        builder.Property(r => r.EaseFactorAfter).IsRequired();
        builder.Property(r => r.ReviewedAt).IsRequired();

        builder.HasIndex(r => new { r.DeckId, r.ReviewedAt });
    }
}

