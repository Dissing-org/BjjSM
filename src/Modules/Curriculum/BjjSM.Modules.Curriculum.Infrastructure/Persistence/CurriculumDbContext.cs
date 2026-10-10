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
