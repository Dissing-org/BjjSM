using BjjSM.Modules.Memberships.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BjjSM.Modules.Memberships.Infrastructure.Persistence;

public class MembershipsDbContext : DbContext
{
    public DbSet<Practitioner> Practitioners => Set<Practitioner>();
    public DbSet<Graduation> Graduations => Set<Graduation>();

    public MembershipsDbContext(DbContextOptions<MembershipsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PractitionerConfiguration());
        modelBuilder.ApplyConfiguration(new GraduationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

public class PractitionerConfiguration : IEntityTypeConfiguration<Practitioner>
{
    public void Configure(EntityTypeBuilder<Practitioner> builder)
    {
        builder.ToTable("Practitioners");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AcademyId).IsRequired();
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Roles).IsRequired();

        // Owned Value Object: InitialRank
        builder.OwnsOne(p => p.InitialRank, rank =>
        {
            rank.Property(r => r.Ladder).HasColumnName("InitialRank_Ladder").IsRequired();
            rank.Property(r => r.BeltName).HasColumnName("InitialRank_BeltName").HasMaxLength(50).IsRequired();
            rank.Property(r => r.Belt).HasColumnName("InitialRank_Belt");
            rank.Property(r => r.KidsBelt).HasColumnName("InitialRank_KidsBelt");
            rank.Property(r => r.Stripes).HasColumnName("InitialRank_Stripes").IsRequired();
        });

        // Owned Value Object: CurrentRank
        builder.OwnsOne(p => p.CurrentRank, rank =>
        {
            rank.Property(r => r.Ladder).HasColumnName("CurrentRank_Ladder").IsRequired();
            rank.Property(r => r.BeltName).HasColumnName("CurrentRank_BeltName").HasMaxLength(50).IsRequired();
            rank.Property(r => r.Belt).HasColumnName("CurrentRank_Belt");
            rank.Property(r => r.KidsBelt).HasColumnName("CurrentRank_KidsBelt");
            rank.Property(r => r.Stripes).HasColumnName("CurrentRank_Stripes").IsRequired();
        });

        // Private backing field collection _graduations
        builder.HasMany(p => p.Graduations)
            .WithOne()
            .HasForeignKey("PractitionerId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Graduations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class GraduationConfiguration : IEntityTypeConfiguration<Graduation>
{
    public void Configure(EntityTypeBuilder<Graduation> builder)
    {
        builder.ToTable("Graduations");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.AcademyId).IsRequired();
        builder.Property(g => g.AwardedAt).IsRequired();
        builder.Property(g => g.AwardedBy).IsRequired();
        builder.Property(g => g.Notes).HasMaxLength(1000);

        // Owned Value Object: Rank
        builder.OwnsOne(g => g.Rank, rank =>
        {
            rank.Property(r => r.Ladder).HasColumnName("Rank_Ladder").IsRequired();
            rank.Property(r => r.BeltName).HasColumnName("Rank_BeltName").HasMaxLength(50).IsRequired();
            rank.Property(r => r.Belt).HasColumnName("Rank_Belt");
            rank.Property(r => r.KidsBelt).HasColumnName("Rank_KidsBelt");
            rank.Property(r => r.Stripes).HasColumnName("Rank_Stripes").IsRequired();
        });
    }
}
