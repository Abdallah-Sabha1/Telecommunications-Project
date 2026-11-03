namespace Aithos.Application.Contracts.Responses;

public sealed class CarbonReportDto
{
    public decimal? TotalCo2Kg { get; set; }
    public decimal ActualElectricityKwh { get; set; }
    public decimal? EmissionFactorKgCo2ePerKwh { get; set; }
    public bool CarbonEvaluated { get; set; }
    public string ReportingPeriod { get; set; } = "";
    public int TotalTowers { get; set; }
    public int TotalReadings { get; set; }
    public List<CarbonContributorDto> TopContributors { get; set; } = [];
    public List<CarbonTrendPointDto> MonthlyTrend { get; set; } = [];
    public CarbonNarrativeDto Narrative { get; set; } = new();
}

public sealed class CarbonContributorDto
{
    public string TowerName { get; init; } = "";
    public string Location { get; init; } = "";
    public decimal ElectricityKwh { get; init; }
    public decimal? TotalKg { get; init; }
}

public sealed class CarbonTrendPointDto
{
    public string Label { get; init; } = "";
    public decimal ElectricityKwh { get; init; }
    public decimal? TotalKg { get; init; }
}

public sealed class CarbonNarrativeDto
{
    public string ExecutiveSummary { get; init; } = "";
    public string Interpretation { get; init; } = "";
    public List<string> Recommendations { get; init; } = [];
}
