namespace Aithos.Application.Contracts.Responses;

public sealed class AnalysisResult
{
    public string Status { get; set; } = "Unknown";
    public string Severity { get; set; } = "Warning";
    public int Confidence { get; set; }
    public string ExecutiveSummary { get; set; } = "";
    public List<string> Evidence { get; set; } = [];
    public List<string> KeyFindings { get; set; } = [];
    public List<AnalysisHypothesis> PossibleCauses { get; set; } = [];
    public string OperationalImpact { get; set; } = "";
    public string EstimatedFinancialLoss { get; set; } = "";
    public string EstimatedCarbonImpact { get; set; } = "";
    public List<string> RecommendedActions { get; set; } = [];
    public AnalysisMetadata Metadata { get; set; } = new();
    public string? Reason { get; set; }
    public string? Recommendation { get; set; }
    public string? PossibleCause { get; set; }

    // Deterministic operational metrics calculated from telemetry and project configuration.
    public decimal? ActualConsumptionKwh { get; set; }
    public decimal? BaselineConsumptionKwh { get; set; }
    public decimal? DeviationPercent { get; set; }
    public string? ObservationWindow { get; set; }
    public string? EstimatedExcessEnergyCost { get; set; }
    public string? EnergyTariff { get; set; }
    public string? CarbonEmissionsFactor { get; set; }
}

public sealed class AnalysisHypothesis
{
    public int Rank { get; init; }
    public string Hypothesis { get; init; } = "";
    public int Confidence { get; init; }
    public string Justification { get; init; } = "";
}

public sealed class AnalysisMetadata
{
    public DateTime AnalyzedAtUtc { get; init; }
    public string Engine { get; init; } = "";
    public string Model { get; init; } = "";
    public string WeatherSource { get; init; } = "";
    public DateTime TelemetryTimestampUtc { get; init; }
}
