using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Aithos.Domain.Import;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class DatasetRepository(AithosDbContext dbContext) : IDatasetRepository
{
    public async Task SaveAsync(
        DatasetImportData dataset,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            // A dataset import is a complete reporting-period reset. Remove
            // derived operational data so old alerts and analyses cannot be
            // mixed with the newly imported 2025 telemetry.
            var oldAlerts = await dbContext.Alerts.ToListAsync(cancellationToken);
            var oldAnalyses = await dbContext.TowerAnalyses.ToListAsync(cancellationToken);
            var oldAiResults = await dbContext.AiAnalysisResults.ToListAsync(cancellationToken);
            dbContext.Alerts.RemoveRange(oldAlerts);
            dbContext.TowerAnalyses.RemoveRange(oldAnalyses);
            dbContext.AiAnalysisResults.RemoveRange(oldAiResults);

            var siteIds = dataset.Towers.Select(row => row.SiteId).ToList();
            var towers = await dbContext.Towers
                .Where(tower => siteIds.Contains(tower.SiteId))
                .ToDictionaryAsync(
                    tower => tower.SiteId,
                    StringComparer.OrdinalIgnoreCase,
                    cancellationToken
                );

            var obsoleteTowers = await dbContext.Towers
                .Where(tower => !siteIds.Contains(tower.SiteId))
                .ToListAsync(cancellationToken);
            dbContext.Towers.RemoveRange(obsoleteTowers);

            foreach (var row in dataset.Towers)
            {
                if (!towers.TryGetValue(row.SiteId, out var tower))
                {
                    tower = new Tower { SiteId = row.SiteId };
                    dbContext.Towers.Add(tower);
                    towers[row.SiteId] = tower;
                }

                tower.Name = row.SiteName;
                tower.Location = row.SiteName;
                tower.Group = row.Group;
                tower.Latitude = row.Latitude;
                tower.Longitude = row.Longitude;
                tower.BaseLoadKwh = row.BaseLoadKwh;
                tower.CoolingCoef = row.CoolingCoef;
                tower.TrafficCoef = row.TrafficCoef;
                tower.Status = "Operational";
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var oldReadings = await dbContext.Readings.ToListAsync(cancellationToken);
            dbContext.Readings.RemoveRange(oldReadings);

            foreach (var row in dataset.Readings)
            {
                var tower = towers[row.SiteId];
                dbContext.Readings.Add(new Reading
                {
                    TowerId = tower.Id,
                    RecordedAt = row.Date,
                    ElectricityConsumption = row.GridKwh,
                    Temperature = row.Temperature,
                    Cdd = row.Cdd,
                    TrafficLoad = row.TrafficAveragePct,
                    TrafficPeakPct = row.TrafficPeakPct,
                    OutageHours = row.OutageHours,
                    // The operational CSV does not provide generator state.
                    // Do not fabricate an OFF state that could create a false
                    // generator-failure diagnosis in the AI analysis.
                    GeneratorStatus = "Unknown",
                    GridStatus = row.OutageHours > 0 ? "OFF" : "ON",
                });
            }

            var oldPrices = await dbContext.MonthlyPrices.ToListAsync(cancellationToken);
            dbContext.MonthlyPrices.RemoveRange(oldPrices);

            foreach (var row in dataset.Prices)
            {
                dbContext.MonthlyPrices.Add(new MonthlyPrice
                {
                    Month = row.Month,
                    ElectricityPrice = row.ElectricityPricePerKwh,
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
