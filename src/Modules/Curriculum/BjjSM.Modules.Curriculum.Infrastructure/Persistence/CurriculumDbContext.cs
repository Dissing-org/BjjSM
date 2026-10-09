using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public class CurriculumDbContext : DbContext
{
    public DbSet<MonthlyPlan> MonthlyPlans => Set<MonthlyPlan>();
    public DbSet<WeeklyFocus> WeeklyFocuses => Set<WeeklyFocus>();
    public DbSet<TechniqueDrill> TechniqueDrills => Set<TechniqueDrill>();

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

        // Unique index per (AcademyId, ProgramId, Year, Month)
        builder.HasIndex(p => new { p.AcademyId, p.ProgramId, p.Year, p.Month }).IsUnique();

        // Backing field navigation _weeks
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
