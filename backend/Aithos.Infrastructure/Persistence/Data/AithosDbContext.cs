using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence;

public class AithosDbContext(DbContextOptions<AithosDbContext> options)
    : DbContext(options)
{
    public DbSet<Tower> Towers => Set<Tower>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<MonthlyPrice> MonthlyPrices => Set<MonthlyPrice>();
    public DbSet<TowerAnalysis> TowerAnalyses => Set<TowerAnalysis>();
    public DbSet<AiAnalysisResult> AiAnalysisResults => Set<AiAnalysisResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tower>().HasIndex((tower) => tower.SiteId).IsUnique();
        modelBuilder.Entity<Tower>().HasIndex((tower) => tower.Name);
        modelBuilder.Entity<Tower>().HasIndex((tower) => tower.Group);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Status).HasMaxLength(30);
        modelBuilder.Entity<Tower>().Property((tower) => tower.SiteId).HasMaxLength(50);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Name).HasMaxLength(120);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Location).HasMaxLength(120);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Group).HasMaxLength(80);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Latitude).HasPrecision(8, 5);
        modelBuilder.Entity<Tower>().Property((tower) => tower.Longitude).HasPrecision(8, 5);
        modelBuilder.Entity<Tower>().Property((tower) => tower.BaseLoadKwh).HasPrecision(10, 2);
        modelBuilder.Entity<Tower>().Property((tower) => tower.CoolingCoef).HasPrecision(10, 4);
        modelBuilder.Entity<Tower>().Property((tower) => tower.TrafficCoef).HasPrecision(10, 4);

        modelBuilder.Entity<Reading>()
            .HasIndex((reading) => new { reading.TowerId, reading.RecordedAt })
            .IsUnique();
        modelBuilder.Entity<Reading>().HasIndex((reading) => reading.RecordedAt);
        modelBuilder.Entity<Reading>().Property((reading) => reading.Cdd).HasPrecision(10, 2);
        modelBuilder.Entity<Reading>().Property((reading) => reading.TrafficPeakPct).HasPrecision(10, 2);
        modelBuilder.Entity<Reading>().Property((reading) => reading.OutageHours).HasPrecision(10, 2);

        modelBuilder
            .Entity<Reading>()
            .Property((reading) => reading.PowerOutput)
            .HasPrecision(10, 2);
        modelBuilder
            .Entity<Reading>()
            .Property((reading) => reading.ElectricityConsumption)
            .HasPrecision(10, 2);
        modelBuilder
            .Entity<Reading>()
            .Property((reading) => reading.Temperature)
            .HasPrecision(6, 2);
        modelBuilder
            .Entity<Reading>()
            .Property((reading) => reading.TrafficLoad)
            .HasPrecision(10, 2);
        modelBuilder
            .Entity<Reading>()
            .Property((reading) => reading.WindSpeed)
            .HasPrecision(6, 2);

        modelBuilder.Entity<MonthlyPrice>().HasIndex((item) => item.Month).IsUnique();
        modelBuilder.Entity<MonthlyPrice>().Property((item) => item.ElectricityPrice).HasPrecision(10, 4);

        modelBuilder.Entity<Reading>()
            .HasOne((reading) => reading.Tower)
            .WithMany((tower) => tower.Readings)
            .HasForeignKey((reading) => reading.TowerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alert>()
            .HasOne((alert) => alert.Tower)
            .WithMany((tower) => tower.Alerts)
            .HasForeignKey((alert) => alert.TowerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TowerAnalysis>().HasIndex((item) => new { item.TowerId, item.CreatedAt });
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.Status).HasMaxLength(20);
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.Severity).HasMaxLength(20);
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.ExecutiveSummary).HasMaxLength(2000);
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.EvidenceJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.KeyFindingsJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.PossibleCausesJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.OperationalImpact).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.EstimatedFinancialLoss).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.EstimatedCarbonImpact).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.RecommendedActionsJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.Engine).HasMaxLength(80);
        modelBuilder.Entity<TowerAnalysis>().Property((item) => item.Model).HasMaxLength(120);
        modelBuilder.Entity<TowerAnalysis>()
            .HasOne((analysis) => analysis.Tower)
            .WithMany((tower) => tower.Analyses)
            .HasForeignKey((analysis) => analysis.TowerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.ActualKwh).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.ExpectedKwh).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.Residual).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.ScoreZ).HasPrecision(12, 6);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.Slope).HasPrecision(12, 6);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.WastedKwh).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.ElectricityPricePerKwh).HasPrecision(12, 6);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.WastedCostJod).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .Property(result => result.CarbonImpactKgCo2e).HasPrecision(12, 4);
        modelBuilder.Entity<AiAnalysisResult>()
            .HasIndex(result => new { result.SiteId, result.ObservationDate })
            .IsUnique();
    }
}
