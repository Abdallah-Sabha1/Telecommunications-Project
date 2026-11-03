using System.Text.Json;
using System.Text.Json.Serialization;
using Aithos.Infrastructure.Persistence;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence;

public sealed class AiImportService(
    AithosDbContext db,
    ILogger<AiImportService> logger,
    IConfiguration configuration
) : IAiImportService
{
    private readonly decimal _emissionFactor =
        configuration.GetValue<decimal?>("Carbon:EmissionFactorKgCo2ePerKwh") ?? 0.4m;
    private readonly bool _emissionFactorApproved =
        configuration.GetValue<bool?>("Carbon:EmissionFactorApproved") ?? true;
    private readonly int _reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    private static readonly HashSet<string> Classifications =
    ["normal", "cooling_degradation", "electrical_waste", "critical_failure"];

    public async Task<AiImportResult> ImportAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default
    )
    {
        if (fileStream is null || (fileStream.CanSeek && fileStream.Length == 0))
        {
            return Failure("A non-empty JSON file is required.");
        }

        try
        {
            
            var rows = await JsonSerializer.DeserializeAsync<List<AiResultRow>>(
                fileStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken
            );

            if (rows is null || rows.Count == 0)
            {
                return Failure("The JSON file does not contain any records.");
            }

            var skippedDieselRecords = rows.RemoveAll(row =>
                string.Equals(row.Classification, "diesel_theft", StringComparison.OrdinalIgnoreCase));
            var skippedOtherYears = rows.RemoveAll(row => row.Date.Year != _reportingYear);
            if (rows.Count == 0)
            {
                return new AiImportResult
                {
                    Success = true,
                    DieselRecordsSkipped = skippedDieselRecords,
                    NonReportingYearRecordsSkipped = skippedOtherYears,
                };
            }

            var errors = Validate(rows);
            if (errors.Count > 0) return new AiImportResult { Errors = errors };

            var siteIds = rows.Select(row => row.SiteId).Distinct().ToList();
            var towers = await db.Towers
                .Where(tower => siteIds.Contains(tower.SiteId))
                .ToDictionaryAsync(tower => tower.SiteId, cancellationToken);

            var missingSites = siteIds.Where(siteId => !towers.ContainsKey(siteId)).ToList();
            if (missingSites.Count > 0)
            {
                return Failure($"Unknown site_id values: {string.Join(", ", missingSites)}");
            }

            var keys = rows.Select(row => new { row.SiteId, row.Date }).ToList();
            var existing = await db.AiAnalysisResults
                .Where(result => siteIds.Contains(result.SiteId))
                .ToListAsync(cancellationToken);
            var existingByKey = existing.ToDictionary(
                item => $"{item.SiteId}|{item.ObservationDate:yyyy-MM-dd}");
            var existingAlerts = await db.Alerts
                .Where(alert => siteIds.Contains(alert.Tower!.SiteId))
                .ToListAsync(cancellationToken);

            var imported = 0;
            var alertsCreated = 0;
            foreach (var row in rows)
            {
                var key = $"{row.SiteId}|{row.Date:yyyy-MM-dd}";
                if (existingByKey.TryGetValue(key, out var existingResult))
                {
                    ApplyEntity(existingResult, row);
                }
                else
                {
                    var entity = ToEntity(row);
                    db.AiAnalysisResults.Add(entity);
                    existingByKey[key] = entity;
                }
                imported++;

                if (row.Classification != "normal")
                {
                    var tower = towers[row.SiteId];
                    var alert = existingAlerts.FirstOrDefault(item =>
                        item.TowerId == tower.Id
                        && item.CreatedAt.Date == row.Date.Date
                        && string.Equals(item.PossibleCause, row.Classification, StringComparison.OrdinalIgnoreCase));
                    if (alert is null)
                    {
                        alert = new Alert { TowerId = tower.Id, CreatedAt = row.Date };
                        db.Alerts.Add(alert);
                        existingAlerts.Add(alert);
                    }

                    alert.Message = row.Reason;
                    alert.Severity = ToSeverity(row.Classification);
                    alert.Status = "open";
                    alert.IsResolved = false;
                    alert.EvidenceSummary = $"Actual {row.ActualKwh:0.##} kWh vs expected {row.ExpectedKwh:0.##} kWh.";
                    alert.EstimatedFinancialLoss = $"JOD {row.WastedCostJod:0.##}";
                    alert.EstimatedCarbonImpact = _emissionFactorApproved
                        ? $"{row.WastedKwh * _emissionFactor:0.##} kg CO2e at the approved project factor"
                        : "Carbon evaluation pending approved emissions factor.";
                    alert.PossibleCause = row.Classification;
                    alert.Recommendation = "Review the tower telemetry and verify the operating conditions.";
                    alertsCreated++;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Imported {Results} AI results and created {Alerts} alerts.", imported, alertsCreated);
            return new AiImportResult
            {
                Success = true,
                ResultsImported = imported,
                AlertsCreated = alertsCreated,
                DieselRecordsSkipped = skippedDieselRecords,
                NonReportingYearRecordsSkipped = skippedOtherYears,
            };
        }
        catch (JsonException)
        {
            return Failure("The uploaded file is not valid JSON in the AI output format.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "AI result import failed.");
            return Failure("AI result import failed. No partial data was saved.");
        }
    }

    private static List<string> Validate(IEnumerable<AiResultRow> rows)
    {
        var errors = new List<string>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.SiteId)) errors.Add("site_id is required.");
            if (row.Date == default) errors.Add("date is required.");
            if (!Classifications.Contains(row.Classification)) errors.Add($"Unsupported classification: {row.Classification}");
            if (string.IsNullOrWhiteSpace(row.Reason)) errors.Add($"reason is required for {row.SiteId}.");
            if (row.ModelVersion != "SRS_v3") errors.Add($"Unsupported model_version for {row.SiteId}: {row.ModelVersion}");
            if (row.ValidationStatus != "validated") errors.Add($"Record is not validated for {row.SiteId}.");
        }
        return errors.Distinct().Take(20).ToList();
    }

    private AiAnalysisResult ToEntity(AiResultRow row) => new()
    {
        SiteId = row.SiteId,
        ObservationDate = row.Date.Date,
        ActualKwh = row.ActualKwh,
        ExpectedKwh = row.ExpectedKwh,
        Residual = row.Residual,
        ScoreZ = row.ScoreZ,
        Classification = row.Classification,
        Slope = row.Slope,
        Reason = row.Reason,
        WastedKwh = row.WastedKwh,
        ElectricityPricePerKwh = row.ElectricityPricePerKwh,
        WastedCostJod = row.WastedCostJod,
        // Always recalculate imported carbon with the approved project factor.
        // The source JSON must not override the deployment configuration.
        CarbonImpactKgCo2e = _emissionFactorApproved
            ? decimal.Round(row.WastedKwh * _emissionFactor, 4)
            : null,
        CarbonEvaluated = _emissionFactorApproved,
        ValidationStatus = row.ValidationStatus,
        ModelVersion = row.ModelVersion,
    };

    private void ApplyEntity(AiAnalysisResult entity, AiResultRow row)
    {
        entity.ActualKwh = row.ActualKwh;
        entity.ExpectedKwh = row.ExpectedKwh;
        entity.Residual = row.Residual;
        entity.ScoreZ = row.ScoreZ;
        entity.Classification = row.Classification;
        entity.Slope = row.Slope;
        entity.Reason = row.Reason;
        entity.WastedKwh = row.WastedKwh;
        entity.ElectricityPricePerKwh = row.ElectricityPricePerKwh;
        entity.WastedCostJod = row.WastedCostJod;
        entity.CarbonImpactKgCo2e = _emissionFactorApproved
            ? decimal.Round(row.WastedKwh * _emissionFactor, 4)
            : null;
        entity.CarbonEvaluated = _emissionFactorApproved;
        entity.ValidationStatus = row.ValidationStatus;
        entity.ModelVersion = row.ModelVersion;
    }

    private static string ToSeverity(string classification) => classification switch
    {
        "critical_failure" => "Critical",
        "electrical_waste" => "High",
        _ => "Warning",
    };

    private static AiImportResult Failure(string message) => new() { Errors = [message] };

    private sealed class AiResultRow
    {
        [JsonPropertyName("site_id")]
        public string SiteId { get; init; } = "";
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }
        [JsonPropertyName("kwh_actual")]
        public decimal ActualKwh { get; init; }
        [JsonPropertyName("kwh_expected")]
        public decimal ExpectedKwh { get; init; }
        [JsonPropertyName("residual")]
        public decimal Residual { get; init; }
        [JsonPropertyName("score_z")]
        public decimal ScoreZ { get; init; }
        [JsonPropertyName("classification")]
        public string Classification { get; init; } = "";
        [JsonPropertyName("slope")]
        public decimal? Slope { get; init; }
        [JsonPropertyName("reason")]
        public string Reason { get; init; } = "";
        [JsonPropertyName("wasted_kwh")]
        public decimal WastedKwh { get; init; }
        [JsonPropertyName("electricity_price_per_kwh")]
        public decimal ElectricityPricePerKwh { get; init; }
        [JsonPropertyName("wasted_cost_jod")]
        public decimal WastedCostJod { get; init; }
        [JsonPropertyName("carbon_impact_kgco2e")]
        public decimal? CarbonImpactKgCo2e { get; init; }
        [JsonPropertyName("carbon_evaluated")]
        public bool CarbonEvaluated { get; init; }
        [JsonPropertyName("validation_status")]
        public string ValidationStatus { get; init; } = "";
        [JsonPropertyName("model_version")]
        public string ModelVersion { get; init; } = "";
    }
}
