using Aithos.Application.Abstractions.Services;
using System.Text.Json;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class AnalysisService(
    ITowerRepository towerRepository,
    IReadingRepository readingRepository,
    ITowerAnalysisRepository analysisRepository,
    IClaudeService claudeService,
    IMonthlyPriceRepository monthlyPriceRepository,
    IConfiguration configuration
) : IAnalysisService
{
    private readonly decimal emissionFactor =
        configuration.GetValue<decimal?>("Carbon:EmissionFactorKgCo2ePerKwh") ?? 0.4m;
    private readonly int reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public async Task<AnalysisResult> AnalyzeAsync(int towerId, CancellationToken cancellationToken)
    {
        var tower = await towerRepository.GetByIdAsync(towerId, false, cancellationToken)
            ?? throw new AnalysisNotFoundException();
        var readings = (await readingRepository.GetRecentByTowerAsync(
            towerId,
            6,
            cancellationToken
        )).Where(reading => reading.RecordedAt.Year == reportingYear).ToList();
        if (readings.Count == 0) throw new AnalysisInputException("The tower has no telemetry to analyze.");

        var result = await claudeService.AnalyzeTowerAsync(
            BuildRequest(tower, readings),
            cancellationToken
        );
        await EnrichDeterministicMetricsAsync(result, tower, readings, cancellationToken);
        ValidateResult(result);

        await analysisRepository.SaveAnalysisAsync(
            CreateAlert(towerId, result),
            CreateRecord(towerId, result),
            cancellationToken
        );
        return result;
    }

    private async Task EnrichDeterministicMetricsAsync(
        AnalysisResult result,
        Tower tower,
        IReadOnlyCollection<Reading> readings,
        CancellationToken cancellationToken
    )
    {
        var ordered = readings.OrderBy(reading => reading.RecordedAt).ToList();
        var latest = ordered[^1];
        var expected = tower.BaseLoadKwh
            + latest.Cdd * tower.CoolingCoef
            + latest.TrafficLoad * tower.TrafficCoef;
        var excess = latest.ElectricityConsumption - expected;
        var deviation = expected == 0
            ? 0
            : decimal.Round(excess / expected * 100m, 1);

        var prices = await monthlyPriceRepository.GetAllAsync(cancellationToken);
        var monthlyPrice = prices
            .Where(price => price.Month.Year == latest.RecordedAt.Year
                && price.Month.Month == latest.RecordedAt.Month)
            .OrderByDescending(price => price.Month)
            .FirstOrDefault()
            ?? prices.OrderByDescending(price => price.Month).FirstOrDefault();

        var pricePerKwh = monthlyPrice?.ElectricityPrice ?? 0m;
        var positiveExcess = Math.Max(0m, excess);

        result.ActualConsumptionKwh = decimal.Round(latest.ElectricityConsumption, 2);
        result.BaselineConsumptionKwh = decimal.Round(expected, 2);
        result.DeviationPercent = deviation;
        result.ObservationWindow =
            $"{(ordered[^1].RecordedAt.Date - ordered[0].RecordedAt.Date).Days + 1} days";
        result.EnergyTariff = pricePerKwh > 0
            ? $"JOD {pricePerKwh:0.000}/kWh from monthly prices"
            : "JOD tariff not configured";
        result.EstimatedExcessEnergyCost = pricePerKwh > 0
            ? $"JOD {positiveExcess * pricePerKwh:0.00} for the latest reading"
            : "Not estimated: JOD tariff not configured";
        result.EstimatedFinancialLoss = result.EstimatedExcessEnergyCost;
        result.CarbonEmissionsFactor =
            $"{emissionFactor:0.###} kg CO2e/kWh approved project factor";
        result.EstimatedCarbonImpact =
            $"{positiveExcess * emissionFactor:0.##} kg CO2e additional impact for the latest reading";
    }

    private static void ValidateResult(AnalysisResult result)
    {
        if (result.Confidence is < 0 or > 100) throw new AnalysisInputException("AI confidence must be between 0 and 100.");
        if (string.IsNullOrWhiteSpace(result.ExecutiveSummary)) throw new AnalysisInputException("AI returned no executive summary.");
        if (result.PossibleCauses.Count == 0 && result.RecommendedActions.Count == 0)
            throw new AnalysisInputException("AI returned no actionable analysis.");
    }

    private static AnalysisRequest BuildRequest(Tower tower, List<Reading> readings) => new(
        tower.Id, tower.SiteId, tower.Name, tower.Location, tower.Group, tower.Latitude, tower.Longitude,
        tower.BaseLoadKwh, tower.CoolingCoef, tower.TrafficCoef,
        readings[0].Temperature, "Tower telemetry", readings[0].RecordedAt, readings.Select(MapReading).ToList());

    private static AnalysisReadingSnapshot MapReading(Reading reading) => new(
        reading.Id, reading.RecordedAt, reading.PowerOutput, reading.ElectricityConsumption,
        reading.Temperature, reading.Cdd, reading.TrafficLoad, reading.TrafficPeakPct, reading.OutageHours,
        reading.WindSpeed, reading.GeneratorStatus, reading.GridStatus);

    private static Alert CreateAlert(int towerId, AnalysisResult result) => new()
    {
        TowerId = towerId,
        Severity = result.Severity,
        Status = "open",
        Message = result.ExecutiveSummary,
        Confidence = result.Confidence,
        EvidenceSummary = string.Join(" | ", result.Evidence.Take(3)),
        EstimatedFinancialLoss = result.EstimatedFinancialLoss,
        EstimatedCarbonImpact = result.EstimatedCarbonImpact,
        PossibleCause = result.PossibleCauses.FirstOrDefault()?.Hypothesis ?? "",
        Recommendation = string.Join(" ", result.RecommendedActions.Take(2)),
        CreatedAt = result.Metadata.TelemetryTimestampUtc,
    };

    private static TowerAnalysis CreateRecord(int towerId, AnalysisResult result) => new()
    {
        TowerId = towerId,
        Status = result.Status,
        Severity = result.Severity,
        Confidence = result.Confidence,
        ExecutiveSummary = result.ExecutiveSummary,
        EvidenceJson = JsonSerializer.Serialize(result.Evidence),
        KeyFindingsJson = JsonSerializer.Serialize(result.KeyFindings),
        PossibleCausesJson = JsonSerializer.Serialize(result.PossibleCauses),
        OperationalImpact = result.OperationalImpact,
        EstimatedFinancialLoss = result.EstimatedFinancialLoss,
        EstimatedCarbonImpact = result.EstimatedCarbonImpact,
        RecommendedActionsJson = JsonSerializer.Serialize(result.RecommendedActions),
        Engine = result.Metadata.Engine,
        Model = result.Metadata.Model,
        TelemetryTimestampUtc = result.Metadata.TelemetryTimestampUtc,
    };
}

public sealed class AnalysisNotFoundException : Exception;
public sealed class AnalysisInputException(string message) : Exception(message);
