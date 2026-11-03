namespace Aithos.Application.Contracts.Responses;

public sealed class DashboardSummaryDto
{
    public decimal FleetHealth { get; set; }

    public int TotalTowers { get; set; }

    public int HealthyTowers { get; set; }

    public int WarningTowers { get; set; }

    public int CriticalTowers { get; set; }

    public decimal CarbonFootprintKg { get; set; }

    public List<DashboardAlertDto> RecentAlerts { get; set; } = [];

    public List<DashboardAnalysisDto> RecentAnalyses { get; set; } = [];

    public List<DashboardRiskTowerDto> TopRiskTowers { get; set; } = [];

    public List<DashboardTrendPointDto> OperationalTrends { get; set; } = [];
}

public sealed class DashboardAlertDto
{
    public int Id { get; set; }

    public int TowerId { get; set; }

    public string TowerName { get; set; } = "";

    public string Location { get; set; } = "";

    public string Severity { get; set; } = "";

    public int Confidence { get; set; }

    public string Summary { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

public sealed class DashboardAnalysisDto
{
    public int Id { get; set; }

    public int TowerId { get; set; }

    public string TowerName { get; set; } = "";

    public string Status { get; set; } = "";

    public string Severity { get; set; } = "";

    public int Confidence { get; set; }

    public string Summary { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

public sealed class DashboardRiskTowerDto
{
    public int TowerId { get; set; }

    public string TowerName { get; set; } = "";

    public string Location { get; set; } = "";

    public string Severity { get; set; } = "";

    public int Confidence { get; set; }
}

public sealed class DashboardTrendPointDto
{
    public string Label { get; set; } = "";

    public decimal ElectricityKwh { get; set; }


    public decimal Temperature { get; set; }

    public decimal TrafficLoad { get; set; }

    public decimal OpenAlerts { get; set; }
}

public sealed class TowerDetailsDto
{
    public int Id { get; set; }

    public string SiteId { get; set; } = "";

    public string Name { get; set; } = "";

    public string Location { get; set; } = "";

    public string Group { get; set; } = "";

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal BaseLoadKwh { get; set; }

    public decimal CoolingCoef { get; set; }

    public decimal TrafficCoef { get; set; }


    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public TowerLatestReadingDto? LatestReading { get; set; }

    public List<TowerReadingDto> RecentReadings { get; set; } = [];

    public List<TowerAlertDto> RecentAlerts { get; set; } = [];

    public List<TowerAnalysisSummaryDto> RecentAnalyses { get; set; } = [];

}

public sealed class TowerLatestReadingDto
{
    public DateTime RecordedAt { get; set; }

    public decimal ElectricityConsumption { get; set; }


    public decimal Temperature { get; set; }

    public decimal TrafficLoad { get; set; }

    public string GeneratorStatus { get; set; } = "";

    public string GridStatus { get; set; } = "";

    public decimal OutageHours { get; set; }

}

public sealed class TowerReadingDto
{
    public int Id { get; set; }

    public DateTime RecordedAt { get; set; }

    public decimal ElectricityConsumption { get; set; }


    public decimal Temperature { get; set; }

    public decimal TrafficLoad { get; set; }

    public string GeneratorStatus { get; set; } = "";

    public string GridStatus { get; set; } = "";
}

public sealed class TowerAlertDto
{
    public int Id { get; set; }

    public string Severity { get; set; } = "";

    public int Confidence { get; set; }

    public string Summary { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

public sealed class TowerAnalysisSummaryDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = "";

    public string Severity { get; set; } = "";

    public int Confidence { get; set; }

    public string Summary { get; set; } = "";
}
