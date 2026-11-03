namespace Aithos.Domain.Entities;

public class TowerAnalysis
{
    public int Id { get; set; }

    public int TowerId { get; set; }

    public Tower? Tower { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime TelemetryTimestampUtc { get; set; }

    public string Status { get; set; } = "Warning";

    public string Severity { get; set; } = "Medium";

    public int Confidence { get; set; }

    public string ExecutiveSummary { get; set; } = "";

    public string EvidenceJson { get; set; } = "[]";

    public string KeyFindingsJson { get; set; } = "[]";

    public string PossibleCausesJson { get; set; } = "[]";

    public string OperationalImpact { get; set; } = "";

    public string EstimatedFinancialLoss { get; set; } = "";

    public string EstimatedCarbonImpact { get; set; } = "";

    public string RecommendedActionsJson { get; set; } = "[]";

    public string Engine { get; set; } = "Telecommunications Project Core AI";

    public string Model { get; set; } = "";
}
