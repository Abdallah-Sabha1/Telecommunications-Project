using Aithos.Application.Abstractions.Services;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class EnergyAnalyticsService(
    ITowerRepository towerRepository,
    IReadingRepository readingRepository,
    IMonthlyPriceRepository priceRepository,
    IConfiguration configuration
) : IEnergyAnalyticsService
{
    private const decimal DefaultElectricityPrice = 0.10m;
    private readonly int reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public async Task<EnergyAnalyticsDto> GetAnalyticsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var towers = await towerRepository.GetAllAsync(cancellationToken);
        var readings = (await readingRepository.GetAllAsync(null, cancellationToken))
            .Where(reading => reading.RecordedAt.Year == reportingYear)
            .ToList();
        var prices = (await priceRepository.GetAllAsync(cancellationToken))
            .Where(price => price.Month.Year == reportingYear)
            .ToList();

        var priceByMonth = prices.ToDictionary(item => ToMonthKey(item.Month));
        var fallbackPrice = prices.LastOrDefault();

        var siteRows = towers
            .Select(tower =>
            {
                var towerReadings = readings.Where(reading => reading.TowerId == tower.Id).ToList();
                var totalCost = SumElectricityCost(towerReadings, priceByMonth, fallbackPrice);

                return new EnergySiteDto
                {
                    TowerId = tower.Id,
                    TowerName = tower.Name,
                    Location = tower.Location,
                    ElectricityKwh = decimal.Round(
                        towerReadings.Sum(reading => reading.ElectricityConsumption),
                        1
                    ),
                    GridCostJod = totalCost,
                    TotalCostJod = totalCost,
                    EfficiencyIndex = totalCost <= 0
                        ? 0
                        : decimal.Round(
                            towerReadings.Sum(reading => reading.ElectricityConsumption)
                            / totalCost,
                            2
                        ),
                };
            })
            .ToList();

        var trend = BuildTrend(readings, priceByMonth, fallbackPrice);
        var reportingDays = readings.Count == 0
            ? 0
            : (readings.Max(reading => reading.RecordedAt).Date - readings.Min(reading => reading.RecordedAt).Date).Days + 1;

        return new EnergyAnalyticsDto
        {
            ReportingPeriod = BuildReportingPeriod(readings),
            TotalCostJod = SumElectricityCost(readings, priceByMonth, fallbackPrice),
            GridCostJod = SumElectricityCost(readings, priceByMonth, fallbackPrice),
            TotalElectricityKwh = decimal.Round(
                readings.Sum(reading => reading.ElectricityConsumption),
                1
            ),
            ReportingDays = reportingDays,
            Trend = trend,
            Sites = siteRows,
        };
    }

    private static List<EnergyTrendPointDto> BuildTrend(
        IReadOnlyCollection<Reading> readings,
        IReadOnlyDictionary<DateTime, MonthlyPrice> priceByMonth,
        MonthlyPrice? fallbackPrice
    )
    {
        return readings
            .GroupBy(reading => ToMonthKey(reading.RecordedAt))
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var cost = SumElectricityCost(group.ToList(), priceByMonth, fallbackPrice);
                return new EnergyTrendPointDto
                {
                    Label = group.Key.ToString("MMM yyyy"),
                    GridCostJod = cost,
                };
            })
            .ToList();
    }

    private static decimal SumElectricityCost(
        IReadOnlyCollection<Reading> readings,
        IReadOnlyDictionary<DateTime, MonthlyPrice> priceByMonth,
        MonthlyPrice? fallbackPrice
    )
    {
        decimal gridCost = 0;

        foreach (var reading in readings)
        {
            var price = priceByMonth.TryGetValue(ToMonthKey(reading.RecordedAt), out var monthly)
                ? monthly
                : fallbackPrice;

            var electricityPrice = price?.ElectricityPrice ?? DefaultElectricityPrice;
            gridCost += reading.ElectricityConsumption * electricityPrice;
        }

        return decimal.Round(gridCost, 2);
    }

    private static DateTime ToMonthKey(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    private static string BuildReportingPeriod(IReadOnlyCollection<Reading> readings)
    {
        if (readings.Count == 0)
        {
            return "No reporting period";
        }

        var start = readings.Min(reading => reading.RecordedAt).Date;
        var end = readings.Max(reading => reading.RecordedAt).Date;
        return $"{start:dd MMM yyyy} – {end:dd MMM yyyy}";
    }
}
