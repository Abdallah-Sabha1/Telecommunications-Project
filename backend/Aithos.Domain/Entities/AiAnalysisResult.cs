namespace Aithos.Domain.Entities;

public class AiAnalysisResult
{
    public int Id { get; set; }
    public string SiteId { get; set; } = "";
    public DateTime ObservationDate { get; set; }
    public decimal ActualKwh { get; set; }
    public decimal ExpectedKwh { get; set; }
    public decimal Residual { get; set; }
    public decimal ScoreZ { get; set; }
    public string Classification { get; set; } = "normal";
    public decimal? Slope { get; set; }
    public string Reason { get; set; } = "";
    public decimal WastedKwh { get; set; }
    public decimal ElectricityPricePerKwh { get; set; }
    public decimal WastedCostJod { get; set; }
    public decimal? CarbonImpactKgCo2e { get; set; }
    public bool CarbonEvaluated { get; set; }
    public string ValidationStatus { get; set; } = "validated";
    public string ModelVersion { get; set; } = "SRS_v3";
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
}
