using Microsoft.EntityFrameworkCore;
using SimTrainer.Api.Domain.Entities;
using SimTrainer.Api.Domain.Enums;

namespace SimTrainer.Api.Infrastructure;

public class SimTrainerDbContext : DbContext
{
    public SimTrainerDbContext(DbContextOptions<SimTrainerDbContext> options) : base(options) { }

    public DbSet<Learner> Learners => Set<Learner>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<CompressionEvent> CompressionEvents => Set<CompressionEvent>();
    public DbSet<VitalSnapshot> VitalSnapshots => Set<VitalSnapshot>();
    public DbSet<ScenarioChange> ScenarioChanges => Set<ScenarioChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Learner>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).HasMaxLength(200).IsRequired();
            e.Property(l => l.Role).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TrainingSession>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.SessionType)
                .HasConversion<string>()
                .HasMaxLength(50);
            e.Property(s => s.OverallScore).HasPrecision(5, 1);
            e.HasOne(s => s.Learner)
                .WithMany(l => l.Sessions)
                .HasForeignKey(s => s.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompressionEvent>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).UseIdentityColumn();
            e.Property(c => c.DepthCm).HasPrecision(4, 2);
            e.Property(c => c.QualityScore).HasPrecision(5, 1);
            e.HasOne(c => c.Session)
                .WithMany(s => s.Compressions)
                .HasForeignKey(c => c.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => c.SessionId);
        });

        modelBuilder.Entity<VitalSnapshot>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).UseIdentityColumn();
            e.HasOne(v => v.Session)
                .WithMany(s => s.Vitals)
                .HasForeignKey(v => v.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => v.SessionId);
        });

        modelBuilder.Entity<ScenarioChange>(e =>
        {
            e.HasKey(sc => sc.Id);
            e.Property(sc => sc.Id).UseIdentityColumn();
            e.Property(sc => sc.ScenarioName).HasMaxLength(100).IsRequired();
            e.Property(sc => sc.PreviousScenario).HasMaxLength(100);
            e.HasOne(sc => sc.Session)
                .WithMany(s => s.ScenarioChanges)
                .HasForeignKey(sc => sc.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(sc => sc.SessionId);
        });
    }
}
