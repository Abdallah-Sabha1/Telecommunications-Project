using Aithos.Application.Abstractions.Services;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class DashboardService(
    ITowerRepository towerRepository,
    IReadingRepository readingRepository,
    IAlertRepository alertRepository,
    ITowerAnalysisRepository analysisRepository,
    IConfiguration configuration
) : IDashboardService
{
    private readonly decimal emissionFactor =
        configuration.GetValue<decimal?>("Carbon:EmissionFactorKgCo2ePerKwh") ?? 0.4m;
    private readonly int reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default
    )
    {
        var towers = await GetTowersAsync(cancellationToken);
        var alerts = await GetOpenAlertsAsync(cancellationToken);
        var analyses = await GetRecentAnalysesAsync(cancellationToken);
        var readings = await GetRecentReadingsAsync(cancellationToken);
        var statusByTower = BuildTowerStatuses(towers, alerts);
        var healthyTowers = statusByTower.Count(item => item.Value == "Healthy");
        var warningTowers = statusByTower.Count(item => item.Value == "Warning");
        var criticalTowers = statusByTower.Count(item => item.Value == "Critical");

        return new DashboardSummaryDto
        {
            TotalTowers = towers.Count,
            HealthyTowers = healthyTowers,
            WarningTowers = warningTowers,
            CriticalTowers = criticalTowers,
            FleetHealth = towers.Count == 0 ? 0 : decimal.Round(healthyTowers * 100m / towers.Count, 1),
            CarbonFootprintKg = CalculateCarbonFootprint(readings),
            RecentAlerts = alerts
                .GroupBy(alert => alert.TowerId)
                .Select(group => group
                    .OrderByDescending(alert => SeverityRank(alert.Severity))
                    .ThenByDescending(alert => alert.CreatedAt)
                    .First())
                .OrderByDescending(alert => SeverityRank(alert.Severity))
                .ThenByDescending(alert => alert.CreatedAt)
                .Take(6)
                .Select(MapAlert)
                .ToList(),
            RecentAnalyses = analyses.Select(MapAnalysis).ToList(),
            TopRiskTowers = BuildTopRiskTowers(alerts),
            OperationalTrends = BuildOperationalTrends(readings, alerts),
        };
    }

    public async Task<TowerDetailsDto?> GetTowerDetailsAsync(
        int towerId,
        CancellationToken cancellationToken = default
    )
    {
        var tower = await towerRepository.GetByIdAsync(
            towerId,
            false,
            cancellationToken
        );

        if (tower is null)
        {
            return null;
        }

        var recentReadings = await readingRepository.GetRecentByTowerAsync(
            towerId,
            10,
            cancellationToken
        );
        var recentAlerts = await alertRepository.GetRecentOpenByTowerAsync(
            towerId,
            5,
            cancellationToken
        );
        var recentAnalyses = await analysisRepository.GetRecentByTowerAsync(
            towerId,
            5,
            cancellationToken
        );

        return new TowerDetailsDto
        {
            Id = tower.Id,
            SiteId = tower.SiteId,
            Name = tower.Name,
            Location = tower.Location,
            Group = tower.Group,
            Latitude = tower.Latitude,
            Longitude = tower.Longitude,
            BaseLoadKwh = tower.BaseLoadKwh,
            CoolingCoef = tower.CoolingCoef,
            TrafficCoef = tower.TrafficCoef,
            Status = ResolveTowerStatus(tower, recentAlerts),
            CreatedAt = tower.CreatedAt,
            LatestReading = recentReadings.FirstOrDefault() is { } latestReading
                ? new TowerLatestReadingDto
                {
                    RecordedAt = latestReading.RecordedAt,
                    ElectricityConsumption = latestReading.ElectricityConsumption,
                    Temperature = latestReading.Temperature,
                    TrafficLoad = latestReading.TrafficLoad,
                    GeneratorStatus = latestReading.GeneratorStatus,
                    GridStatus = latestReading.GridStatus,
                    OutageHours = latestReading.OutageHours,
                }
                : null,
            RecentReadings = recentReadings.Select(reading => new TowerReadingDto
            {
                Id = reading.Id,
                RecordedAt = reading.RecordedAt,
                ElectricityConsumption = reading.ElectricityConsumption,
                Temperature = reading.Temperature,
                TrafficLoad = reading.TrafficLoad,
                GeneratorStatus = reading.GeneratorStatus,
                GridStatus = reading.GridStatus,
            }).ToList(),
            RecentAlerts = recentAlerts.Select(MapTowerAlert).ToList(),
            RecentAnalyses = recentAnalyses.Select(MapTowerAnalysis).ToList(),
        };
    }

    private static DashboardAlertDto MapAlert(Alert alert)
    {
        return new DashboardAlertDto
        {
            Id = alert.Id,
            TowerId = alert.TowerId,
            TowerName = alert.Tower?.Name ?? $"Tower {alert.TowerId}",
            Location = alert.Tower?.Location ?? "Unknown",
            Severity = alert.Severity,
            Confidence = alert.Confidence,
            Summary = alert.Message,
            CreatedAt = alert.CreatedAt,
        };
    }

    private static DashboardAnalysisDto MapAnalysis(TowerAnalysis analysis)
    {
        return new DashboardAnalysisDto
        {
            Id = analysis.Id,
            TowerId = analysis.TowerId,
            TowerName = analysis.Tower?.Name ?? $"Tower {analysis.TowerId}",
            Status = analysis.Status,
            Severity = analysis.Severity,
            Confidence = analysis.Confidence,
            Summary = analysis.ExecutiveSummary,
            CreatedAt = analysis.CreatedAt,
        };
    }

    private static TowerAlertDto MapTowerAlert(Alert alert)
    {
        return new TowerAlertDto
        {
            Id = alert.Id,
            Severity = alert.Severity,
            Confidence = alert.Confidence,
            Summary = alert.Message,
            CreatedAt = alert.CreatedAt,
        };
    }

    private static TowerAnalysisSummaryDto MapTowerAnalysis(TowerAnalysis analysis)
    {
        return new TowerAnalysisSummaryDto
        {
            Id = analysis.Id,
            CreatedAt = analysis.CreatedAt,
            Status = analysis.Status,
            Severity = analysis.Severity,
            Confidence = analysis.Confidence,
            Summary = analysis.ExecutiveSummary,
        };
    }

    private static int SeverityRank(string? severity)
    {
        return severity?.ToLowerInvariant() switch
        {
            "critical" => 3,
            "warning" => 2,
            "info" => 1,
            _ => 2,
        };
    }

    private async Task<List<Tower>> GetTowersAsync(CancellationToken cancellationToken)
    {
        return await towerRepository.GetAllAsync(cancellationToken);
    }

    private async Task<List<Alert>> GetOpenAlertsAsync(CancellationToken cancellationToken)
    {
        return await alertRepository.GetOpenAsync(cancellationToken);
    }

    private async Task<List<TowerAnalysis>> GetRecentAnalysesAsync(
        CancellationToken cancellationToken
    )
    {
        return await analysisRepository.GetRecentAsync(6, cancellationToken);
    }

    private async Task<List<Reading>> GetRecentReadingsAsync(CancellationToken cancellationToken)
    {
        return (await readingRepository.GetAllAsync(null, cancellationToken))
            .Where(reading => reading.RecordedAt.Year == reportingYear)
            .ToList();
    }

    private static bool IsOperationalTower(Tower tower)
    {
        return tower.Status.Equals("Operational", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountBySeverity(IEnumerable<Alert> alerts, string severity)
    {
        return alerts
            .Where(alert => alert.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase))
            .Select(alert => alert.TowerId)
            .Distinct()
            .Count();
    }

    private static Dictionary<int, string> BuildTowerStatuses(
        IReadOnlyCollection<Tower> towers,
        IReadOnlyCollection<Alert> alerts
    )
    {
        var statusByTower = towers.ToDictionary(tower => tower.Id, _ => "Healthy");

        foreach (var group in alerts.GroupBy(alert => alert.TowerId))
        {
            var highest = group.MaxBy(alert => SeverityRank(alert.Severity));
            statusByTower[group.Key] = highest is null ? "Healthy" : ResolveAlertStatus(highest.Severity);
        }

        return statusByTower;
    }

    private static string ResolveTowerStatus(Tower tower, IReadOnlyCollection<Alert> alerts)
    {
        var highest = alerts.MaxBy(alert => SeverityRank(alert.Severity));
        return highest is null ? "Healthy" : ResolveAlertStatus(highest.Severity);
    }

    private static string ResolveAlertStatus(string? severity)
    {
        return severity?.Trim().ToLowerInvariant() switch
        {
            "critical" or "high" or "severe" => "Critical",
            "warning" or "medium" or "degraded" => "Warning",
            _ => "Healthy",
        };
    }

    private static decimal CalculateFleetHealth(IReadOnlyCollection<Tower> towers)
    {
        if (towers.Count == 0)
        {
            return 0;
        }

        var operationalCount = towers.Count(IsOperationalTower);
        return decimal.Round(operationalCount * 100m / towers.Count, 1);
    }

    private decimal CalculateCarbonFootprint(IReadOnlyCollection<Reading> readings)
    {
        return decimal.Round(
            readings.Sum(reading => reading.ElectricityConsumption) * emissionFactor,
            1
        );
    }

    private static List<DashboardRiskTowerDto> BuildTopRiskTowers(List<Alert> alerts)
    {
        return alerts
            .GroupBy(alert => alert.TowerId)
            .Select(group =>
            {
                var first = group.First();
                return new DashboardRiskTowerDto
                {
                    TowerId = group.Key,
                    TowerName = first.Tower?.Name ?? $"Tower {group.Key}",
                    Location = first.Tower?.Location ?? "Unknown",
                    Severity = group.MaxBy(item => SeverityRank(item.Severity))?.Severity ?? "Warning",
                    Confidence = group.Max(item => item.Confidence),
                };
            })
            .OrderByDescending(item => SeverityRank(item.Severity))
            .ThenByDescending(item => item.Confidence)
            .Take(5)
            .ToList();
    }

    private static List<DashboardTrendPointDto> BuildOperationalTrends(
        List<Reading> readings,
        List<Alert> alerts
    )
    {
        var alertCountByDate = alerts
            .GroupBy(alert => alert.CreatedAt.Date)
            .ToDictionary(
                group => group.Key,
                group => group.Select(alert => alert.TowerId).Distinct().Count()
            );

        return readings
            .GroupBy(reading => reading.RecordedAt.Date)
            .OrderBy(group => group.Key)
            .Select(group => new DashboardTrendPointDto
            {
                Label = group.Key.ToString("dd MMM"),
                ElectricityKwh = decimal.Round(group.Sum(reading => reading.ElectricityConsumption), 1),
                Temperature = decimal.Round(group.Average(reading => reading.Temperature), 1),
                TrafficLoad = decimal.Round(group.Average(reading => reading.TrafficLoad), 1),
                OpenAlerts = alertCountByDate.GetValueOrDefault(group.Key),
            })
            .TakeLast(7)
            .ToList();
    }
}
