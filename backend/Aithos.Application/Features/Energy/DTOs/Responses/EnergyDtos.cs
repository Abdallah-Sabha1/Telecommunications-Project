namespace Aithos.Application.Contracts.Responses;

public sealed class EnergyAnalyticsDto
{
    public string ReportingPeriod { get; set; } = "";

    public decimal GridCostJod { get; set; }

    public decimal TotalCostJod { get; set; }

    public decimal TotalElectricityKwh { get; set; }

    public int ReportingDays { get; set; }

    public List<EnergyTrendPointDto> Trend { get; set; } = [];

    public List<EnergySiteDto> Sites { get; set; } = [];
}

public sealed class EnergyTrendPointDto
{
    public string Label { get; set; } = "";

    public decimal GridCostJod { get; set; }
}

public sealed class EnergySiteDto
{
    public int TowerId { get; set; }

    public string TowerName { get; set; } = "";

    public string Location { get; set; } = "";

    public decimal ElectricityKwh { get; set; }

    public decimal GridCostJod { get; set; }

    public decimal TotalCostJod { get; set; }

    public decimal EfficiencyIndex { get; set; }
}
